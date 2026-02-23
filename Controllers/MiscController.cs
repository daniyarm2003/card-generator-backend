using Microsoft.AspNetCore.Mvc;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("/api")]
    public class MiscController : ControllerBase
    {
        [HttpGet("hc")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "ok",
                timestamp = DateTime.UtcNow
            });
        }
    }
}