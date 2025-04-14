using Microsoft.AspNetCore.Mvc;

namespace mvc.Controllers
{
    public class ErrorController : Controller
    {
        [Route("404")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotFound()
        {
            return View("NotFound");
        }

        [Route("403")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Unauthorized()
        {
            return View("Unauthorized");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return RedirectToAction("NotFound");
                case 403:
                    return RedirectToAction("Unauthorized");
                default:
                    return View("Error");
            }
        }
    }
}
