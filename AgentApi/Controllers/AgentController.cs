using Domain.DTOs;
using Domain.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly ILogger<AgentController> _logger;
    private readonly IChatCommandHandler _chatCommandHandler;

    public AgentController(ILogger<AgentController> logger, IChatCommandHandler chatCommandHandler)
    {
        _logger = logger;
        _chatCommandHandler = chatCommandHandler;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            _logger.LogInformation("Chat request received");
            
            var response = await _chatCommandHandler.HandleAsync(request);
            
            _logger.LogInformation("Chat response sent successfully. AgentThreadId: {AgentThreadId}", response.AgentThreadId);
            return Ok(response);
        }
        catch (AzureConfigurationException ex)
        {
            _logger.LogError(ex, "Azure configuration error in chat request");
            return StatusCode(500, new { 
                error = "Configuration Error", 
                message = ex.Message,
                details = "Please check your Azure configuration settings"
            });
        }
        catch (ConfigurationException ex)
        {
            _logger.LogError(ex, "Configuration error in chat request");
            return StatusCode(500, new { 
                error = "Configuration Error", 
                message = ex.Message,
                details = "Please check your application configuration"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in chat request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

   
}
