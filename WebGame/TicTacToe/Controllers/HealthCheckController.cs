using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TicTacToe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet("health-check")]
        public IActionResult Get()
        {
            // Trả về trạng thái OK để cho biết rằng API đang hoạt động
            return Ok(new { status = "API running" });
        }
    }
}
