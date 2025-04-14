using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.ViewModels.ReviewVM;
using mvc.Hubs;
using System.Security.Claims;

namespace mvc.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBussinessRepository _businessRepository;
        private readonly IHubContext<ReviewHub> hubcontext;

        public ReviewController(IReviewRepository reviewRepository, IBussinessRepository businessRepository, IHubContext<ReviewHub> hubcontext)
        {
            _reviewRepository = reviewRepository;
            _businessRepository = businessRepository;
            this.hubcontext = hubcontext;
        }

        public async Task<IActionResult> GetAllByBussniss(int bussnissId)
        {
            List<Review> reviews = (await _reviewRepository.GetByBusinessIdAsync(bussnissId)).ToList();
            return View(reviews);
        }

        public IActionResult Add()
        {
            return View();
        }

        public async Task<IActionResult> SaveAdd(AddReviewVM reviewVM)
        {
            Business business = await _businessRepository.GetByIdAsync(reviewVM.BusinessId);
            if (business == null)
            {
                ModelState.AddModelError("BusinessId", "Business not found");
            }
            if (_reviewRepository.IsExist(reviewVM.Email))
            {
                ModelState.AddModelError("Email", "Email already add an review you ");
            }
            if (ModelState.IsValid)
            {
                Review review = new Review
                {
                    Email = reviewVM.Email,
                    BusinessId = reviewVM.BusinessId,
                    Rating = reviewVM.Rating,
                    Comment = reviewVM.Comment
                };
                await _reviewRepository.AddAsync(review);
                await _reviewRepository.SaveAsync();
                await hubcontext.Clients.All.SendAsync("NewReviewArrived", reviewVM);

                return RedirectToAction("GetAllByBussniss", new { bussnissId = review.BusinessId });
            }

            return View("Add", reviewVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadReviewsCount()
        {
            int count = await _reviewRepository.GetAll(r => !r.IsRead).CountAsync();
            return Json(count);
        }

        public async Task<IActionResult> GetUnreadReviews()
        {
            var unreadReviews = await _reviewRepository.GetAll(r => !r.IsRead).ToListAsync();

            foreach (var review in unreadReviews)
            {
                review.IsRead = true;
            }

            await _reviewRepository.SaveAsync();

            return View(unreadReviews);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBusinessReviewNotifications()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new object[0]);
            }

            // Get all businesses owned by current user
            var userBusinessIds = _businessRepository.GetAll()
                .Where(b => b.OwnerId == userId)
                .Select(b => b.Id)
                .ToList();

            if (!userBusinessIds.Any())
            {
                return Json(new object[0]);
            }

            // Get reviews for these businesses, ordered by most recent
            var reviews = await _reviewRepository.GetAll(r =>
                    userBusinessIds.Contains(r.BusinessId))
                .OrderByDescending(r => r.CreatedAt)
                .Take(10) // Limit to 10 most recent
                .Select(r => new
                {
                    id = r.Id,
                    businessId = r.BusinessId,
                    businessName = r.Business.Name,
                    email = r.Email,
                    rating = r.Rating,
                    comment = r.Comment,
                    isRead = r.IsRead,
                    timeAgo = GetTimeAgo(r.CreatedAt)
                })
                .ToListAsync();

            return Json(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadReviewsCountForUserBusinesses()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(0);
            }

            // Get all businesses owned by current user
            var userBusinessIds = _businessRepository.GetAll()
                .Where(b => b.OwnerId == userId)
                .Select(b => b.Id)
                .ToList();

            // Count unread reviews for these businesses
            int count = await _reviewRepository.GetAll(r =>
                    userBusinessIds.Contains(r.BusinessId) && !r.IsRead)
                .CountAsync();

            return Json(count);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    return Json(new { success = false, message = "Review not found" });
                }

                // Handle both user-owned business reviews and admin reviews
                if (User.IsInRole("Admin"))
                {
                    // Admin can mark any review as read
                    review.IsRead = true;
                    await _reviewRepository.UpdateAsync(review);
                    await _reviewRepository.SaveAsync();
                    return Json(new { success = true });
                }
                else
                {
                    // Regular users can only mark reviews for their own businesses
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var business = await _businessRepository.GetByIdAsync(review.BusinessId);

                    if (business == null || business.OwnerId != userId)
                    {
                        return Json(new { success = false, message = "You don't have permission to mark this review as read" });
                    }

                    review.IsRead = true;
                    await _reviewRepository.UpdateAsync(review);
                    await _reviewRepository.SaveAsync();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get all businesses owned by current user
            var userBusinessIds = _businessRepository.GetAll()
                .Where(b => b.OwnerId == userId)
                .Select(b => b.Id)
                .ToList();

            // Get all unread reviews for these businesses
            var unreadReviews = await _reviewRepository.GetAll(r =>
                    userBusinessIds.Contains(r.BusinessId) && !r.IsRead)
                .ToListAsync();

            foreach (var review in unreadReviews)
            {
                review.IsRead = true;
            }

            await _reviewRepository.SaveAsync();

            return Ok();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
        }

        public async Task<IActionResult> Edit(int id)
        {
            Review review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            EditReviewVM reviewVM = new EditReviewVM
            {
                Id = review.Id,
                Email = review.Email,
                Rating = review.Rating,
                Comment = review.Comment
            };

            return View(reviewVM);
        }

        public async Task<IActionResult> SaveEditAsync(int id, EditReviewVM reviewVM)
        {

            if (ModelState.IsValid)
            {
                Review review = await _reviewRepository.GetByIdAsync(id);
                review.Email = reviewVM.Email;
                review.Rating = reviewVM.Rating;
                review.Comment = reviewVM.Comment;
                _reviewRepository.SaveAsync();

                return RedirectToAction("GetAllByBussniss", new { bussnissId = review.BusinessId });
            }
            return View("Edit", reviewVM);
        }

        public async Task<ActionResult> Delete(int id)/////admin
        {
            Review review = await _reviewRepository.GetByIdAsync(id);
            await _reviewRepository.DeleteAsync(id);
            await _reviewRepository.SaveAsync();
            return RedirectToAction("GetAllByBussniss", new { bussnissId = review.BusinessId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(Review review)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill in all required fields correctly." });
            }

            try
            {
                // Check if business exists
                var business = await _businessRepository.GetByIdAsync(review.BusinessId);
                if (business == null)
                {
                    return Json(new { success = false, message = "Business not found." });
                }

                // Check if user already reviewed this business
                var userEmail = review.Email;
                var existingReview = await _reviewRepository.GetByBusinessAndEmailAsync(review.BusinessId, userEmail);
                
                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.CreatedAt = DateTime.UtcNow;
                    existingReview.IsRead = false; // Mark as unread for new notification
                    
                    await _reviewRepository.UpdateAsync(existingReview);
                    await _reviewRepository.SaveAsync();
                    
                    // Notify through SignalR
                    var reviewVM = new AddReviewVM
                    {
                        Email = existingReview.Email,
                        BusinessId = existingReview.BusinessId,
                        Rating = existingReview.Rating,
                        Comment = existingReview.Comment
                    };
                    
                    await hubcontext.Clients.All.SendAsync("NewReviewArrived", reviewVM);
                    
                    return Json(new { success = true, message = "Your review has been updated successfully!" });
                }
                
                // Set creation date and read status
                review.CreatedAt = DateTime.UtcNow;
                review.IsRead = false;
                
                // Save new review
                await _reviewRepository.AddAsync(review);
                await _reviewRepository.SaveAsync();
                
                // Notify through SignalR
                var newReviewVM = new AddReviewVM
                {
                    Email = review.Email,
                    BusinessId = review.BusinessId,
                    Rating = review.Rating,
                    Comment = review.Comment
                };
                
                await hubcontext.Clients.All.SendAsync("NewReviewArrived", newReviewVM);
                
                return Json(new { success = true, message = "Your review has been submitted successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while submitting your review. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReviewsPartial(int businessId)
        {
            try
            {
                // Get the business
                var business = await _businessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    return PartialView("_ReviewsPartial", new List<Review>());
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Set the owner status
                ViewBag.IsOwner = !string.IsNullOrEmpty(userId) && userId == business.OwnerId;
                
                // Get reviews for this business
                var reviews = await _reviewRepository.GetByBusinessIdAsync(businessId);
                
                // Calculate average rating
                double averageRating = business.GetAverageRating();
                
                // Calculate rating percentages
                var ratingPercentages = business.GetRatingPercentages();
                
                ViewBag.AverageRating = averageRating;
                ViewBag.RatingPercentages = ratingPercentages;
                
                return PartialView("_ReviewsPartial", reviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching reviews: {ex.Message}");
                return PartialView("_ReviewsPartial", new List<Review>());
            }
        }
    }
}
