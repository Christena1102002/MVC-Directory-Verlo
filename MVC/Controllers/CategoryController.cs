using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using mvc.Repositories;

namespace mvc.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryReposiotry categoryReposiotry;

        public CategoryController(ICategoryReposiotry categoryReposiotry)
        {
            this.categoryReposiotry = categoryReposiotry;
        }


        public IActionResult Index(int pageNumbr, int size)
        {
            IQueryable<Category> categories = categoryReposiotry.GetAll(pageNumbr, size)
                    .Include(f => f.CategoryFeatures);

            return View("Index", categories);
        }

        public IActionResult New()
        {
            return View(new CategoryViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> SaveNew(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Data is not valid.";
                return View("New", model);
            }

            var category = new Category
            {
                Name = model.Name,
                Icon = model.Icon,
                CategoryFeatures = model.Features.Select(f => new CategoryFeatures
                {
                    Name = f.Name
                }).ToList()
            };
            await categoryReposiotry.AddAsync(category);
            await categoryReposiotry.SaveAsync();
            TempData["success"] = "Category added successfully.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Details(int id)
        {
            var category = await categoryReposiotry.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View("Details", category);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await categoryReposiotry.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new EditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Icone = category.Icon,
                Features = category.CategoryFeatures
                    .Select(f => new EditFeatureViewModel
                    {
                        FeatureID = f.Id,
                        NameFeature = f.Name
                    })
                    .ToList()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveEdit(EditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var category = await categoryReposiotry.GetByIdAsync(model.Id);
            if (category == null)
            {
                return NotFound();
            }

            
            category.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Icone) && model.Icone != category.Icon)
            {
                category.Icon = model.Icone;
            }

            
            foreach (var featureModel in model.Features)
            {
                if (featureModel.FeatureID == 0)  // إذا كان الـ FeatureID صفرًا، يعني فيتشر جديدة
                {
                    category.CategoryFeatures.Add(new CategoryFeatures
                    {
                        Name = featureModel.NameFeature
                    });
                }
                else
                {
                    var existingFeature = category.CategoryFeatures.FirstOrDefault(f => f.Id == featureModel.FeatureID);
                    if (existingFeature != null)
                    {
                        existingFeature.Name = featureModel.NameFeature;  
                    }
                }
            }

            
            categoryReposiotry.Update(category);
            await categoryReposiotry.SaveAsync();

            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteFeature(int categoryId, int featureId)
        {
            var category = await categoryReposiotry.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var feature = category.CategoryFeatures.FirstOrDefault(f => f.Id == featureId);
            if (feature != null)
            {
                category.CategoryFeatures.Remove(feature);
                await categoryReposiotry.SaveAsync();
                return Ok();
            }

            return BadRequest("Feature not found.");
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryFeatures(int categoryId)
        {
            var category = await categoryReposiotry.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var features = category.CategoryFeatures.Select(f => new
            {
                f.Id,
                f.Name
            }).ToList();

            return Json(features);
        }
  


        public async Task<IActionResult> Delete(int id)
        {
            var category = await categoryReposiotry.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await categoryReposiotry.DeleteAsync(id);
            await categoryReposiotry.SaveAsync();
            return RedirectToAction("Index");
        }
    }
}