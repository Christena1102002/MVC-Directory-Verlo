using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvc.Repositories
{
    public class ReviewRepository : GeniricRepository<int, Review>, IReviewRepository
    {
        protected readonly ProjectContext dbContext;

        public ReviewRepository(ProjectContext context) : base(context)
        {
            this.dbContext = context;
        }

        public bool IsExist(string email)
        {
            return dbSet.Any(r => r.Email == email);
        }

        public async Task<IEnumerable<Review>> GetByBusinessIdAsync(int businessId)
        {
            return await dbSet
                .Where(r => r.BusinessId == businessId)
                .Include(r => r.Business)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetByBusinessAndEmailAsync(int businessId, string email)
        {
            return await dbSet
                .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Email == email);
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10)
        {
            return await dbSet
                .Include(r => r.Business)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetUnreadReviewsAsync()
        {
            return await dbSet
                .Include(r => r.Business)
                .Where(r => !r.IsRead)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForBusinessAsync(int businessId)
        {
            var reviews = await dbSet
                .Where(r => r.BusinessId == businessId)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<int> GetReviewCountForBusinessAsync(int businessId)
        {
            return await dbSet
                .CountAsync(r => r.BusinessId == businessId);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int businessId)
        {
            var reviews = await dbSet
                .Where(r => r.BusinessId == businessId)
                .ToListAsync();

            var distribution = new Dictionary<int, int>();
            
            // Initialize all ratings (1-5) with zero count
            for (int i = 1; i <= 5; i++)
            {
                distribution[i] = 0;
            }

            // Count reviews for each rating
            foreach (var review in reviews)
            {
                distribution[review.Rating]++;
            }

            return distribution;
        }
        
        // Use 'new' instead of 'override' to hide the base implementation
        public new async Task<Review> GetByIdAsync(int id)
        {
            return await dbSet
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(Review review)
        {
            dbContext.Entry(review).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }
    }
}
