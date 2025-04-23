using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace mvc.Controllers
{
    public class LibraryController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public LibraryController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        [Route("/Library/CheckSignalR")]
        public IActionResult CheckSignalR()
        {
            string signalRPath = Path.Combine(_hostEnvironment.WebRootPath, "lib", "signalr", "dist", "browser", "signalr.min.js");
            bool signalRExists = System.IO.File.Exists(signalRPath);
            
            string message = signalRExists 
                ? "SignalR library found at the correct location." 
                : "SignalR library is missing! The fallback CDN should be used.";

            string restoreCommand = "dotnet tool install -g Microsoft.Web.LibraryManager.Cli\r\nlibman restore";
            
            ViewBag.SignalRExists = signalRExists;
            ViewBag.SignalRPath = signalRPath;
            ViewBag.RestoreCommand = restoreCommand;
            
            return View();
        }
    }
}
