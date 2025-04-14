using mvc.Models;

namespace mvc.RepoInterfaces
{
    public interface ICategoryReposiotry:IGeniricRepository<int,Category>
    {
        //  Task<Category> GetByNameAsync(string name);

        public interface ICategoryRepository
        {
            Task<CategoryFeatures> GetFeatureByIdAsync(int featureId);
            Task DeleteFeatureAsync(CategoryFeatures feature);
        }


    }

}
