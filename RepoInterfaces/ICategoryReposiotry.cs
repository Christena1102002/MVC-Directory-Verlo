using mvc.Models;

namespace mvc.RepoInterfaces
{
    public interface ICategoryReposiotry:IGeniricRepository<int,Category>
    {
       

        public interface ICategoryRepository
        {
            Task<CategoryFeatures> GetFeatureByIdAsync(int featureId);
            Task DeleteFeatureAsync(CategoryFeatures feature);
        }


    }

}
