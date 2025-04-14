using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mvc.Controllers
{
    public class AdminChatController : Controller
    {
        [Authorize(Roles = "Admin")]
      
            public IActionResult Index()
            {
                return View();
            }
        
    }
}
