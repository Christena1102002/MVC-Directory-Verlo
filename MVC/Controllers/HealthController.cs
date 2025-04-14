using Microsoft.AspNetCore.Mvc;

namespace mvc.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        [Route("api/health/check")]
        public IActionResult ApiCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpHead]
        [Route("api/health/check")]
        public IActionResult ApiHeadCheck()
        {
            return Ok();
        }

        [HttpGet]
        [Route("health-check")]
        public IActionResult HealthCheck()
        {
            return View("Check");
        }
    }
}
