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
    public class BusinessAdminController : Controller
    {
        private readonly IBussinessRepository DbBusiness;
        private readonly ICategoryReposiotry Dbcategory;
        private readonly IBusinessFeaturesRepoisitory DbBusinessFeatures;
        private readonly ProjectContext _context;

        public BusinessAdminController(IBussinessRepository bussinessRepository, 
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
        public IActionResult GetAll(int packetId = 0, int pageNumber = 0, int size = 0)
        {
            List<Business> businessList = DbBusiness.GetAll(packetId, pageNumber, size).ToList();
            return View("GetAll", businessList);
        }

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
                    return View("Add", busFromReq);
                }

                if (ModelState.IsValid)
                {
                    // Check for duplicate business name
                    bool isExist = await DbBusiness.IsBusinessExistAsync(busFromReq.Name);
                    if (isExist)
                    {
                        busFromReq.categories = Dbcategory.GetAll().ToList();
                        busFromReq.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
                        ModelState.AddModelError("Name", "This name is already in use");                    
                        return View("Add", busFromReq);
                    }

                    // Use a database transaction to ensure integrity
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            // Set default Regular package
                            int defaultPackageId = 1; // Regular free package

                            Console.WriteLine($"Saving new business: {busFromReq.Name}, Category: {busFromReq.CategoryId}, Owner: {userId}");

                            // Create business object
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

                            // Save business
                            await DbBusiness.AddAsync(NewBusiness);
                            int saveResult = await DbBusiness.SaveAsync();
                            if (saveResult <= 0)
                            {
                                throw new Exception("Failed to save business record to database");
                            }
                            
                            Console.WriteLine($"Business saved successfully with result: {saveResult}");

                            // Get ID of the added business
                            int newBusinessId = DbBusiness.getIdByName(NewBusiness.Name);
                            if (newBusinessId <= 0)
                            {
                                throw new Exception("Retrieved ID is invalid");
                            }
                            Console.WriteLine($"Retrieved business ID: {newBusinessId}");

                            // Handle business features
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

                            // Handle opening hours
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
                            return RedirectToAction("GetAll");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"Transaction rolled back: {ex.Message}");
                            throw; // Rethrow to be handled by outer catch
                        }
                    }
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
            }

            // Reload required data for the form in case of error
            busFromReq.categories = Dbcategory.GetAll().ToList();
            busFromReq.businessesNameList = DbBusiness.GetAll().Select(b => b.Name).ToList();
            return View("Add", busFromReq);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var business = await DbBusiness.GetByIdAsync(id);

            if (business == null)
            {
                TempData["Error"] = "Business not found";
                return NotFound();
            }

            await DbBusiness.DeleteAsync(id);
            await DbBusiness.SaveAsync();
            TempData["Success"] = "Business has been deleted successfully";
            return RedirectToAction("GetAll");
        }

        public async Task<IActionResult> Edit(int id)
        {        
            var business = await DbBusiness.GetByIdAsync(id);

            if (business == null)
            {
                TempData["Error"] = "Business not found";
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

            // Start a single transaction
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
                    return NotFound();
                }

                // Check ownership
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (businessToUpdate.OwnerId != currentUserId && !User.IsInRole("Admin"))
                {
                    TempData["Error"] = "You don't have permission to edit this business";
                    return Forbid();
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
                return RedirectToAction("GetAll");
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
        public IActionResult GetBusinessByUserId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }
                
                List<Business> businesses = DbBusiness.GetAll().Where(b => b.OwnerId == id).ToList();
                if (businesses.Count == 0)
                {
                    ViewBag.Message = "No businesses found for this user";
                    return View("myBusiness", new List<Business>());
                }
                
                return View("myBusiness", businesses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user businesses: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the businesses");
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
