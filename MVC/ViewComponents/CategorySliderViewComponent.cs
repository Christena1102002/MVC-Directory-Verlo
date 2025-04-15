using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using System.Threading.Tasks;

namespace mvc.ViewComponents
{
    public class CategorySliderViewComponent : ViewComponent
    {
        private readonly ICategoryReposiotry _categoryRepository;

        public CategorySliderViewComponent(ICategoryReposiotry categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            
            var categories = await _categoryRepository.GetAll().ToListAsync();
            return View(categories);
        }
    }
}
