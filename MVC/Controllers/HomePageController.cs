using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace mvc.Controllers
{
    public class HomePageController : Controller
    {


      [Authorize(Roles = "User")]
        public IActionResult Index()
        {
            string id=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value; //id_user

            ViewBag.UserId = id;
           
            return View();
        }
    }
}
