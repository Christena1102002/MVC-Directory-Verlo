using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Enums;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.ViewModels;
using mvc.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace mvc.Controllers
{
    public class BusinessController : Controller
    {
        private readonly IBussinessRepository DbBusiness;
        private readonly ICategoryReposiotry Dbcategory;
        private readonly IBusinessFeaturesRepoisitory DbBusinessFeatures;
        private readonly ProjectContext _context;

        public BusinessController(IBussinessRepository bussinessRepository, 
                                IBusinessFeaturesRepoisitory businessFeaturesReposiotry, 
                                ICategoryReposiotry categoryReposiotry,
                                ProjectContext context)
        {
            this.DbBusiness = bussinessRepository;
            this.DbBusinessFeatures = businessFeaturesReposiotry;
            this.Dbcategory = categoryReposiotry;
            this._context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // return all business if pageNumber or size == Null 
        // or return some business if we want to divide the view to some pages
        public IActionResult GetAll(int pageNumber = 1, int size = 12, string searchTerm = "", string category = "", 
                                    string rating = "", string package = "", string status = "", string sort = "nameAsc")
        {
            try
            {
                // Start with all businesses
                var query = DbBusiness.GetAll();
                
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(b => 
                        b.Name.ToLower().Contains(searchTerm) || 
                        (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
                }
                
                // Apply category filter
                if (!string.IsNullOrWhiteSpace(category) && int.TryParse(category, out int categoryId))
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }
                
                // Apply rating filter
                if (!string.IsNullOrWhiteSpace(rating) && float.TryParse(rating, out float minRating))
                {
                    query = query.Where(b => b.Reviews.Any() && b.Reviews.Average(r => r.Rating) >= minRating);
                }
                
                // Apply package filter
                if (!string.IsNullOrWhiteSpace(package) && int.TryParse(package, out int packageId))
                {
                    query = query.Where(b => b.PackageId == packageId);
                }
                
                // Apply status filter
                if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out bool isActive))
                {
                    query = query.Where(b => b.IsActive == isActive);
                }
                
                // Count total for pagination
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (float)size);
                
                // Apply sorting
                query = ApplySorting(query, sort);
                
                // Include related entities
                query = query.Include(b => b.Category)
                            .Include(b => b.BusinessFeatures)
                            .Include(b => b.Reviews);
                
                // Apply pagination
                var businesses = query.Skip((pageNumber - 1) * size)
                                     .Take(size)
                                     .ToList();
                
                // Load all categories for filters
                ViewBag.Categories = Dbcategory.GetAll().ToList();
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.ItemsPerPage = size;
                ViewBag.TotalItems = totalItems;
                
                // For AJAX requests, return partial view
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BusinessesPartial", businesses);
                }
                
                return View("GetAll", businesses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                
                // Return empty results rather than error
                ViewBag.Categories = Dbcategory.GetAll().ToList();
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.ItemsPerPage = size;
                ViewBag.TotalItems = 0;
                
                return View("GetAll", new List<Business>());
            }
        }

        private IQueryable<Business> ApplySorting(IQueryable<Business> query, string sort)
        {
            switch (sort)
            {
                case "nameDesc":
                    return query.OrderByDescending(b => b.Name);
                case "ratingDesc":
                    return query.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0);
                case "ratingAsc":
                    return query.OrderBy(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0);
                case "newest":
                    return query.OrderByDescending(b => b.Id); // Assuming Id correlates with creation time
                case "oldest":
                    return query.OrderBy(b => b.Id);
                case "nameAsc":
                default:
                    return query.OrderBy(b => b.Name);
            }
        }

        [Authorize (Roles ="User")]
        public IActionResult Add()
        {
            BusinessViewModel business = new BusinessViewModel();
            business.categories = Dbcategory.GetAll().ToList();
            business.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
            
            try
            {
                // إضافة قائمة الباقات (مع مراعاة الخصائص المتاحة في Package class)
                var packages = _context.Packages.ToList();
                if (packages != null && packages.Any())
                {
                    business.Packages = packages;
                }
                else
                {
                    // إضافة باقة افتراضية إذا لم يتم العثور على أي باقات
                    business.Packages = new List<Package> { 
                        new Package { 
                            Id = 1, 
                            Name = "Regular",
                            // Remove the Price property which doesn't exist
                            MonthlyPrice = 0,
                            YearlyPrice = 0
                            // Description is also removed since it might not exist
                        } 
                    };
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ ولكن لا تدع التطبيق يتوقف
                Console.WriteLine($"Error loading packages: {ex.Message}");
                business.Packages = new List<Package>();
            }
            
            return View("Add", business);
        }

        [HttpPost]
        public async Task<IActionResult> Save(BusinessViewModel busFromReq)
        {
            try
            {
                // Check if user is logged in
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "You must be logged in to add a business");
                    busFromReq.categories = Dbcategory.GetAll().ToList();
                    busFromReq.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
                    TempData["Error"] = "You must be logged in to add a business";
                    return View("Add", busFromReq);
                }

                if (ModelState.IsValid)
                {
                    // التحقق من عدم تكرار اسم العمل
                    bool isExist = await DbBusiness.IsBusinessExistAsync(busFromReq.Name);
                    if (isExist)
                    {
                        busFromReq.categories = Dbcategory.GetAll().ToList();
                        busFromReq.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
                        ModelState.AddModelError("Name", "This name is already in use");
                        TempData["Error"] = "Business name is already in use";
                        return View("Add", busFromReq);
                    }

                    // استخدام معاملة قاعدة بيانات واحدة لضمان تكامل العملية
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            // تعيين باقة Regular المجانية افتراضيًا
                            int defaultPackageId = 1; // باقة Regular المجانية

                            Console.WriteLine($"Saving new business: {busFromReq.Name}, Category: {busFromReq.CategoryId}, Owner: {userId}");

                            // إنشاء كائن العمل التجاري
                            Business NewBusiness = new Business
                            {
                                Name = busFromReq.Name,
                                Longitude = busFromReq.Longitude,
                                Latitude = busFromReq.Latitude,
                                CategoryId = busFromReq.CategoryId,
                                Description = busFromReq.Description,
                                MainImage = busFromReq.MainImage,
                                Address = busFromReq.Address,
                                OwnerId = userId,
                                SubscriptionEndDate = DateTime.UtcNow.AddMonths(1),
                                IsActive = true,
                                PackageId = defaultPackageId,
                                BusinessType = BusinessType.Regular
                            };

                            // حفظ العمل التجاري
                            await DbBusiness.AddAsync(NewBusiness);
                            int saveResult = await DbBusiness.SaveAsync();
                            if (saveResult <= 0)
                            {
                                throw new Exception("Failed to save business record to database");
                            }
                            
                            Console.WriteLine($"Business saved successfully with result: {saveResult}");

                            // الحصول على معرف العمل المضاف
                            int newBusinessId = DbBusiness.getIdByName(NewBusiness.Name);
                            if (newBusinessId <= 0)
                            {
                                throw new Exception("Retrieved ID is invalid");
                            }
                            Console.WriteLine($"Retrieved business ID: {newBusinessId}");

                            // معالجة ميزات العمل التجاري
                            if (busFromReq.BusinessFeatures != null && busFromReq.BusinessFeatures.Any())
                            {
                                int featuresAdded = 0;
                                Console.WriteLine($"Processing {busFromReq.BusinessFeatures.Count} business features");
                                
                                foreach (var feature in busFromReq.BusinessFeatures)
                                {
                                    if (!string.IsNullOrWhiteSpace(feature.Name))
                                    {
                                        feature.BusinessId = newBusinessId;
                                        await DbBusinessFeatures.AddAsync(feature);
                                        featuresAdded++;
                                    }
                                }

                                if (featuresAdded > 0)
                                {
                                    int featuresSaveResult = await DbBusinessFeatures.SaveAsync();
                                    Console.WriteLine($"Saved {featuresAdded} features with result: {featuresSaveResult}");
                                }
                            }

                            // معالجة ساعات العمل
                            var openingHourRepository = HttpContext.RequestServices.GetService<IOpeningHourRepository>();
                            if (openingHourRepository == null)
                            {
                                // Log the error
                                Console.WriteLine("Error: Could not resolve IOpeningHourRepository");
                                // Handle the missing service appropriately - either throw an exception or continue without it
                            } 
                            else 
                            {
                                // Use the service
                                if (busFromReq.OpeningHours != null && busFromReq.OpeningHours.Any())
                                {
                                    Console.WriteLine($"Processing {busFromReq.OpeningHours.Count} business hours records");
                                    
                                    int hoursAdded = 0;
                                    
                                    foreach (var hour in busFromReq.OpeningHours)
                                    {
                                        hour.BusinessId = newBusinessId;
                                        await openingHourRepository.AddAsync(hour);
                                        hoursAdded++;
                                    }

                                    if (hoursAdded > 0)
                                    {
                                        await openingHourRepository.SaveAsync();
                                        Console.WriteLine($"Saved {hoursAdded} business hours records");
                                    }
                                }
                            }

                            await transaction.CommitAsync();
                            TempData["Success"] = "Business has been created successfully!";
                            return RedirectToAction("GetBusinessByUserId", new { id = userId });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"Transaction rolled back: {ex.Message}");
                            TempData["Error"] = "Failed to save business. Please try again.";
                            throw; // إعادة رمي الاستثناء للتعامل معه في كتلة catch الخارجية
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Please correct the errors in the form.";
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving business: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                ModelState.AddModelError("", "An error occurred while saving the business. Please try again.");
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            // إعادة تحميل البيانات المطلوبة للنموذج في حالة الخطأ
            busFromReq.categories = Dbcategory.GetAll().ToList();
            busFromReq.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
            return View("Add", busFromReq);
        }

        // Add this new method to get category features
        [HttpGet]
        public async Task<IActionResult> GetCategoryFeatures(int categoryId)
        {
            try
            {
                var category = await Dbcategory.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound();
                }

                // Fixed anonymous type issue by explicitly returning empty list of same type
                var features = category.CategoryFeatures != null && category.CategoryFeatures.Any() 
                    ? category.CategoryFeatures.Select(f => new { id = f.Id, name = f.Name }).ToList() 
                    : new List<object>().Select(o => new { id = 0, name = "" }).ToList();

                return Json(features);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category features: {ex.Message}");
                return StatusCode(500, "Failed to load category features");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var business = await DbBusiness.GetByIdAsync(id);

            if (business == null)
            {
                return NotFound();
            }

            await DbBusiness.DeleteAsync(id);
            await DbBusiness.SaveAsync();
            return RedirectToAction("GetAll");
        }

        public async Task<IActionResult> Edit(int id)
        {        
            var business = await DbBusiness.GetByIdAsync(id);

            if (business == null)
            {
                return NotFound();
            }

            BusinessViewModel viewModel = new BusinessViewModel
            {
                Id = business.Id,
                Name = business.Name,
                CategoryId = business.CategoryId,
                Description = business.Description,
                MainImage = business.MainImage,
                Latitude = business.Latitude,
                Longitude = business.Longitude,
                Address = business.Address,
                IsActive = business.IsActive,
                SubscriptionEndDate = business.SubscriptionEndDate,
                categories = Dbcategory.GetAll().ToList(),
                BusinessFeatures = DbBusinessFeatures.GetAll(b => b.BusinessId == business.Id).ToList(),
                businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList()
            };

            // الحصول على ساعات العمل
            var openingHourRepository = HttpContext.RequestServices.GetService<IOpeningHourRepository>();
            if (openingHourRepository == null)
            {
                // Log the error
                Console.WriteLine("Error: Could not resolve IOpeningHourRepository");
                // Handle the missing service appropriately - either throw an exception or continue without it
            } 
            else 
            {
                // Use the service
                viewModel.OpeningHours = await openingHourRepository.GetByBusinessIdAsync(id);
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEdit(BusinessViewModel busFromReq)
        {
            if (!ModelState.IsValid)
            {
                await ReloadBusinessData(busFromReq);
                return View("Edit", busFromReq);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check for duplicate name
                var nameExists = await DbBusiness.GetAll()
                    .AnyAsync(b => b.Id != busFromReq.Id && b.Name == busFromReq.Name);

                if (nameExists)
                {
                    ModelState.AddModelError("Name", "This name is already in use");
                    await ReloadBusinessData(busFromReq);
                    return View("Edit", busFromReq);
                }

                // Get and update basic data
                var businessToUpdate = await DbBusiness.GetByIdAsync(busFromReq.Id);
                if (businessToUpdate == null) 
                {
                    TempData["Error"] = "Business not found";
                    return NotFound("Business not found");
                }

                // Check ownership
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!businessToUpdate.OwnerId.Equals(currentUserId) && !User.IsInRole("Admin"))
                {
                    TempData["Error"] = "You don't have permission to edit this business";
                    return Forbid("You don't have permission to edit this business");
                }

                // Update properties
                businessToUpdate.Name = busFromReq.Name;
                businessToUpdate.Longitude = busFromReq.Longitude;
                businessToUpdate.Latitude = busFromReq.Latitude;
                businessToUpdate.CategoryId = busFromReq.CategoryId;
                businessToUpdate.Description = busFromReq.Description;
                businessToUpdate.MainImage = busFromReq.MainImage;
                businessToUpdate.Address = busFromReq.Address;
                businessToUpdate.IsActive = busFromReq.IsActive;

                // Update features
                await UpdateBusinessFeatures(busFromReq);

                // Update opening hours without a new transaction
                await UpdateOpeningHoursWithoutTransaction(busFromReq.Id, busFromReq.OpeningHours);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Business has been updated successfully";
                // Fix redirect to use GetBusinessByUserId instead of myBusiness
                return RedirectToAction("GetBusinessByUserId", new { id = currentUserId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await ReloadBusinessData(busFromReq);
                return View("Edit", busFromReq);
            }
        }

        private async Task UpdateOpeningHoursWithoutTransaction(int businessId, List<OpeningHour> openingHours)
        {
            if (openingHours == null || !openingHours.Any()) return;

            // حذف الساعات القديمة
            var existingHours = await _context.OpeningHours
                .Where(h => h.BusinessId == businessId)
                .ToListAsync();

            _context.OpeningHours.RemoveRange(existingHours);

            // إضافة الساعات الجديدة
            foreach (var hour in openingHours)
            {
                hour.BusinessId = businessId;
                _context.OpeningHours.Add(hour);
            }
        }

        private async Task ReloadBusinessData(BusinessViewModel model)
        {
            model.categories = await Dbcategory.GetAll().ToListAsync();
            model.BusinessFeatures = await DbBusinessFeatures.GetAll(b => b.BusinessId == model.Id).ToListAsync();
            model.businessesNameList = await DbBusiness.GetAll().Select(b => b.Name).ToListAsync();
        }

        private async Task UpdateBusinessFeatures(BusinessViewModel model)
        {
            if (model.BusinessFeatures == null || !model.BusinessFeatures.Any()) return;

            var existingFeatures = await DbBusinessFeatures.GetAll(f => f.BusinessId == model.Id).ToListAsync();
            foreach (var feature in existingFeatures)
            {
                await DbBusinessFeatures.DeleteAsync(feature.Id);
            }

            foreach (var feature in model.BusinessFeatures.Where(f => !string.IsNullOrWhiteSpace(f.Name)))
            {
                feature.BusinessId = model.Id;
                await DbBusinessFeatures.AddAsync(feature);
            }
        }

        public IActionResult GetBusinessByUserId(string id, int page = 1, int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }
                
                // Load all categories for the filter dropdown
                ViewBag.Categories = Dbcategory.GetAll().ToList();
                
                // Get total count for pagination
                int totalItems = DbBusiness.GetAll().Count(b => b.OwnerId == id);
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Get paginated results
                List<Business> businesses = DbBusiness.GetAll()
                    .Where(b => b.OwnerId == id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.ItemsPerPage = pageSize;
                
                if (businesses.Count == 0 && page == 1)
                {
                    // No businesses found, but still show the page
                    return View("myBusiness", new List<Business>());
                }
                
                return View("myBusiness", businesses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user businesses: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving businesses");
            }
        }

        [HttpGet]
        public IActionResult GetBusinessByUserIdPaged(string id, int page = 1, string searchTerm = "", string status = "all", string package = "all", string category = "all", int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }
                
                // Load all categories for the filter dropdown if it's not an AJAX request
                if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                {
                    ViewBag.Categories = Dbcategory.GetAll().ToList();
                }
                
                // Get all businesses for the user first
                var allBusinesses = DbBusiness.GetAll().Where(b => b.OwnerId == id);
                
                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    allBusinesses = allBusinesses.Where(b => 
                        b.Name.ToLower().Contains(searchTerm) || 
                        (b.Category != null && b.Category.Name.ToLower().Contains(searchTerm)) ||
                        (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
                }
                
                if (status != "all")
                {
                    bool isActive = status == "active";
                    allBusinesses = allBusinesses.Where(b => b.IsActive == isActive);
                }
                
                if (package != "all")
                {
                    int packageId = package == "regular" ? 1 : package == "premium" ? 2 : 3;
                    allBusinesses = allBusinesses.Where(b => b.PackageId == packageId);
                }
                
                // Add category filter
                if (category != "all" && int.TryParse(category, out int categoryId))
                {
                    allBusinesses = allBusinesses.Where(b => b.CategoryId == categoryId);
                }
                
                // Calculate pagination
                int totalItems = allBusinesses.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Get paginated results
                var pagedBusinesses = allBusinesses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.ItemsPerPage = pageSize;
                
                // If it's an AJAX request, return partial view
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BusinessGrid", pagedBusinesses);
                }
                
                return View("myBusiness", pagedBusinesses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching paginated businesses: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving businesses");
            }
        }

        public async Task<IActionResult> GetBusinessById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid business ID");
                }
                
                Business business = await DbBusiness.GetByIdAsync(id);
                if (business == null)
                {
                    return NotFound("Business not found");
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Set the owner status
                ViewBag.IsOwner = !string.IsNullOrEmpty(userId) && userId == business.OwnerId;
                
                // Get reviews for this business
                var reviewRepository = HttpContext.RequestServices.GetService<IReviewRepository>();
                if (reviewRepository != null)
                {
                    var reviews = await reviewRepository.GetByBusinessIdAsync(id);
                    
                    // Calculate average rating
                    double averageRating = business.GetAverageRating();
                    
                    // Calculate rating percentages
                    var ratingPercentages = business.GetRatingPercentages();
                    
                    ViewBag.AverageRating = averageRating;
                    ViewBag.RatingPercentages = ratingPercentages;
                }
                
                // Get owner businesses count
                var ownerBusinessCount = await DbBusiness.GetAll(b => b.OwnerId == business.OwnerId).CountAsync();
                ViewBag.OwnerBusinessCount = ownerBusinessCount;
                
                // Set the business ID for review form
                ViewBag.BusinessId = id;
                
                return View(business);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching business: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the business");
            }
        }

        [HttpGet]
        public IActionResult GetUserBusinessIds()
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User is not logged in", data = new int[0] });
                }
                
                var businessIds = DbBusiness.GetAll()
                    .Where(b => b.OwnerId == userId)
                    .Select(b => b.Id)
                    .ToList();
                
                return Json(new { success = true, data = businessIds });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user business IDs: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while retrieving business IDs", data = new int[0] });
            }
        }

        [HttpGet]
        public IActionResult Search(string searchTerm, int pageNumber = 0, int size = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return RedirectToAction("GetAll");
                }
                
                var businesses = DbBusiness.GetAll(
                    b => b.Name.Contains(searchTerm) || b.Description.Contains(searchTerm),
                    pageNumber, 
                    size
                ).ToList();
                
                ViewBag.SearchTerm = searchTerm;
                
                return View("SearchResults", businesses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching businesses: {ex.Message}");
                TempData["Error"] = "An error occurred while searching for businesses";
                return RedirectToAction("GetAll");
            }
        }
    }
}
