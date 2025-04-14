using mvc.Models;

namespace mvc.Repositories
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId);
        Task<bool> IsFavoriteAsync(string userId, int businessId);
        Task<bool> ToggleFavoriteAsync(string userId, int businessId);
        Task<int> GetFavoritesCountAsync(int businessId);
        Task<int> GetUserFavoritesCountAsync(string userId);
        Task RemoveAllUserFavoritesAsync(string userId);
    }
}
