using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;

namespace mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBussinessRepository _businessRepository;
    private readonly ICategoryReposiotry _categoryRepository;

    public HomeController(
        ILogger<HomeController> logger,
        IBussinessRepository businessRepository,
        ICategoryReposiotry categoryRepository)
    {
        _logger = logger;
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        try
        {
            // Number of businesses per page
            int pageSize = 6;
            
            // Get premium/sponsored businesses (those with non-free packages)
            var query = _businessRepository.GetAll(b => 
                b.IsActive && 
                b.PackageId > 1
            );
            
            // Ensure needed relations are included
            query = query.Include(b => b.Category)
                .Include(b => b.BusinessFeatures)
                .Include(b => b.Reviews);
                
            var sponsoredBusinesses = await query
                .OrderByDescending(b => b.PackageId) // Higher package first
                .ThenByDescending(b => b.SubscriptionEndDate) // Latest subscription first
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Get categories for dropdown - ensure this doesn't return null
            var categories = await _categoryRepository.GetAll().ToListAsync();
            ViewBag.Categories = categories ?? new List<Category>();
            
            // Get business counts for each category
            var categoryBusinessCounts = new Dictionary<int, int>();
            foreach (var category in categories)
            {
                var count = await _businessRepository.GetAll(b => b.IsActive && b.CategoryId == category.Id).CountAsync();
                categoryBusinessCounts[category.Id] = count;
            }
            ViewBag.CategoryBusinessCounts = categoryBusinessCounts;
            
            // Pagination info
            var totalBusinesses = await _businessRepository.GetAll(b => 
                b.IsActive && 
                b.PackageId > 1 
            ).CountAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBusinesses / pageSize);
            
            return View(sponsoredBusinesses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            ViewBag.Categories = new List<Category>(); // Ensure ViewBag has non-null values
            ViewBag.CategoryBusinessCounts = new Dictionary<int, int>();
            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = 1;
            return View(new List<Business>());
        }
    }

    // Improved search suggestions with categorization 
    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2)
            return Json(new List<string>());
            
        try
        {
            // Search in business names and descriptions
            var businessSuggestions = await _businessRepository.GetAll(b => 
                b.IsActive && 
                (b.Name.Contains(query) || b.Description.Contains(query))
            )
            .Select(b => new { text = b.Name })
            .Take(5)
            .ToListAsync();
            
            // Search in categories - add (Category) suffix
            var categorySuggestions = await _categoryRepository.GetAll()
                .Where(c => c.Name.Contains(query))
                .Select(c => new { text = c.Name + " (Category)" })
                .Take(3)
                .ToListAsync();
            
            // Search in business features - add (Feature) suffix
            var featureSuggestions = await _businessRepository.GetAll(b => 
                b.IsActive && 
                b.BusinessFeatures.Any(f => f.Name.Contains(query))
            )
            .SelectMany(b => b.BusinessFeatures.Where(f => f.Name.Contains(query)))
            .Select(f => new { text = f.Name + " (Feature)" })
            .Distinct()
            .Take(3)
            .ToListAsync();
            
            var combinedSuggestions = businessSuggestions
                .Union(categorySuggestions)
                .Union(featureSuggestions)
                .Take(10)
                .ToList();
            
            return Json(combinedSuggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions");
            return Json(new List<string>());
        }
    }

    // Method to handle AJAX pagination for sponsored businesses
    [HttpGet]
    public async Task<IActionResult> SponsoredBusinesses(int page = 1)
    {
        try
        {
            int pageSize = 6;
            
            // Get only sponsored businesses (package > 1)
            var businesses = await _businessRepository.GetAll(b => 
                b.IsActive && 
                b.PackageId > 1 // Ensure only sponsored businesses
            )
            .Include(b => b.Category)
            .Include(b => b.BusinessFeatures)
            .Include(b => b.Reviews)
            .OrderByDescending(b => b.PackageId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
            return PartialView("_BusinessesPartial", businesses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sponsored businesses for AJAX pagination");
            return Content("Error loading businesses. Please try again.");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
