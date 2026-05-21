using Microsoft.AspNetCore.Mvc;

namespace Notaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeployInfoController : ControllerBase
    {
        [HttpGet("info")]
        public IActionResult GetDeployInfo()
        {
            return Ok(new
            {
                status     = "✅ Running",
                deployedAt = Environment.GetEnvironmentVariable("DEPLOY_TIME") ?? "unknown",
                buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "unknown",
                version    = Environment.GetEnvironmentVariable("APP_VERSION") ?? "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }
    }
}