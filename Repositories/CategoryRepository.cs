using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using System.Linq;
using System.Threading.Tasks;

namespace mvc.Repositories
{
    public class CategoryRepository : GeniricRepository<int, Category>, ICategoryReposiotry
    {
        private readonly ProjectContext _context;

        public CategoryRepository(ProjectContext context) : base(context)
        {
            _context = context;
        }

        public  IQueryable<Category> GetAll(int? pageNumber, int? size)
        {
            IQueryable<Category> query = _context.Categories.Include(c => c.CategoryFeatures);

            if (pageNumber.HasValue && size.HasValue)
            {
                query = query.Skip((pageNumber.Value - 1) * size.Value).Take(size.Value);
            }

            return query;
        }

      
        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                                 .Include(c => c.CategoryFeatures)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category obj)
        {
           await _context.Categories.AddAsync(obj);
      
        await _context.SaveChangesAsync(); 
        }

        //public async Task<Category> GetByNameAsync(string name)
        //{
        //    return await _context.Categories
        //                         .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        //}

        public async Task<CategoryFeatures> GetFeatureByIdAsync(int featureId)
        {
            return await _context.CategoryFeatures.FindAsync(featureId);
        }

        public async Task DeleteFeatureAsync(CategoryFeatures feature)
        {
            _context.CategoryFeatures.Remove(feature);
            await _context.SaveChangesAsync();
        }




    }
}
