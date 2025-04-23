using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mvc.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        public IActionResult UserChat()
        {
            return View();
        }
    }
}
