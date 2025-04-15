using mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace mvc.RepoInterfaces
{
    public interface IReviewRepository : IGeniricRepository<int, Review>
    {
       
        Task<IEnumerable<Review>> GetByBusinessIdAsync(int businessId);
        Task<Review> GetByBusinessAndEmailAsync(int businessId, string email);
        Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10);
        Task<IEnumerable<Review>> GetUnreadReviewsAsync();
        Task<double> GetAverageRatingForBusinessAsync(int businessId);
        Task<int> GetReviewCountForBusinessAsync(int businessId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int businessId);
        bool IsExist(string email);
        Task UpdateAsync(Review review);
    }
}
