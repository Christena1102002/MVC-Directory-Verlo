using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly ProjectContext _context;

        public FavoriteRepository(ProjectContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId)
        {
            return await _context.Favorites
                .Include(f => f.Business)
                    .ThenInclude(b => b.Category)
                .Include(f => f.Business)
                    .ThenInclude(b => b.Reviews)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsFavoriteAsync(string userId, int businessId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.BusinessId == businessId);
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int businessId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessId == businessId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return false; // Removed from favorites
            }
            else
            {
                var newFavorite = new Favorite
                {
                    UserId = userId,
                    BusinessId = businessId,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Favorites.Add(newFavorite);
                await _context.SaveChangesAsync();
                return true; // Added to favorites
            }
        }

        public async Task<int> GetFavoritesCountAsync(int businessId)
        {
            return await _context.Favorites
                .CountAsync(f => f.BusinessId == businessId);
        }

        public async Task<int> GetUserFavoritesCountAsync(string userId)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId);
        }

        public async Task RemoveAllUserFavoritesAsync(string userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();
                
            _context.Favorites.RemoveRange(favorites);
            await _context.SaveChangesAsync();
        }
    }
}
