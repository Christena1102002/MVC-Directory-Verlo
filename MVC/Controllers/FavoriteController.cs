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
                    TempData["Error"] = "يجب تسجيل الدخول لعرض المفضلة";
                    return RedirectToAction("Login", "Account");
                }

                var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
                
                // عرض عدد المفضلة في ViewBag
                ViewBag.FavoritesCount = favorites.Count();

                return View(favorites);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء جلب المفضلة: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteViewModel model)
        {
            if (model == null || model.BusinessId <= 0)
            {
                return Json(new { success = false, message = "طلب غير صالح" });
            }

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                // التحقق من وجود العمل
                bool businessExists = await _businessRepository.IsBusinessExistByIdAsync(model.BusinessId);
                if (!businessExists)
                {
                    return Json(new { success = false, message = "العمل التجاري غير موجود" });
                }

                // تبديل حالة المفضلة
                bool isFavorite = await _favoriteRepository.ToggleFavoriteAsync(userId, model.BusinessId);
                string message = isFavorite ? "تمت إضافة العمل للمفضلة" : "تمت إزالة العمل من المفضلة";
                
                return Json(new { success = true, isFavorite, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
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
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                await _favoriteRepository.RemoveAllUserFavoritesAsync(userId);
                
                TempData["Success"] = "تم مسح جميع المفضلة بنجاح";
                return RedirectToAction(nameof(MyFavorites));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء مسح المفضلة: " + ex.Message;
                return RedirectToAction(nameof(MyFavorites));
            }
        }
    }

    public class FavoriteViewModel
    {
        public int BusinessId { get; set; }
    }
}