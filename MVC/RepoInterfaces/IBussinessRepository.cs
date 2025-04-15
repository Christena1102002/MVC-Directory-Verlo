using mvc.Enums;
using mvc.Models;

namespace mvc.RepoInterfaces
{
    public interface IBussinessRepository:IGeniricRepository<int, Business>
    {
        int getIdByName(string name);
        IQueryable<Business> GetAll(int p, int size=0, int  pageNumber=1);
        Task<bool> IsBusinessExistAsync(string name);
        Task<bool> IsBusinessExistByIdAsync(int id);
        Task<Category> GetCategoryByBusinessIdAsync(int businessId);
    }
}
