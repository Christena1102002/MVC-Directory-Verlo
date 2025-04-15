using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Repositories;
using mvc.RepoInterfaces;
using System.Security.Claims;

namespace mvc.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IBussinessRepository _businessRepository;

        public FavoriteController(
            IFavoriteRepository favoriteRepository, 
            IBussinessRepository businessRepository)
        {
            _favoriteRepository = favoriteRepository;
            _businessRepository = businessRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyFavorites()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must login to view your favorites list";
                    return RedirectToAction("Login", "Account");
                }

                var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
                
                ViewBag.FavoritesCount = favorites.Count();

                return View(favorites);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while fetching favorites: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteViewModel model)
        {
            if (model == null || model.BusinessId <= 0)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "You must login first" });
                }

                bool businessExists = await _businessRepository.IsBusinessExistByIdAsync(model.BusinessId);
                if (!businessExists)
                {
                    return Json(new { success = false, message = "Business does not exist" });
                }

                bool isFavorite = await _favoriteRepository.ToggleFavoriteAsync(userId, model.BusinessId);
                string message = isFavorite ? "Business added to favorites" : "Business removed from favorites";
                
                return Json(new { success = true, isFavorite, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IsFavorite(int businessId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { isFavorite = false });
                }

                bool isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, businessId);
                return Json(new { isFavorite });
            }
            catch (Exception)
            {
                return Json(new { isFavorite = false });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveAllFavorites()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "You must login first" });
                }

                await _favoriteRepository.RemoveAllUserFavoritesAsync(userId);
                
                TempData["Success"] = "All favorites were successfully removed";
                return RedirectToAction(nameof(MyFavorites));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while removing favorites: " + ex.Message;
                return RedirectToAction(nameof(MyFavorites));
            }
        }
    }

    public class FavoriteViewModel
    {
        public int BusinessId { get; set; }
    }
}