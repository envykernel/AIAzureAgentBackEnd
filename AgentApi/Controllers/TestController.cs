using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("cors")]
    public IActionResult TestCors()
    {
        return Ok(new { 
            message = "CORS is working!",
            timestamp = DateTime.UtcNow,
            origin = Request.Headers["Origin"].ToString()
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }
}

