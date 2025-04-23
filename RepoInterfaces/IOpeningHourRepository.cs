using mvc.Models;
using System.Linq.Expressions;

namespace mvc.RepoInterfaces
{
    public interface IOpeningHourRepository : IGeniricRepository<int, OpeningHour>
    {
        Task<List<OpeningHour>> GetByBusinessIdAsync(int businessId);
        Task DeleteByBusinessIdAsync(int businessId);
        
        // Add the missing method for batch updating hours
        Task UpdateBusinessHoursAsync(int businessId, List<OpeningHour> openingHours);
    }
}
