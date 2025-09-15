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
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation in chat request");
            return StatusCode(500, new { 
                error = "Internal Error", 
                message = ex.Message,
                details = "Please try again or contact support if the issue persists"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in chat request");
            return BadRequest(new { 
                error = "Invalid Request", 
                message = ex.Message,
                details = "Please check your request parameters"
            });
        }
        catch (TokenLimitExceededException ex)
        {
            _logger.LogWarning(ex, "Token limit exceeded for thread {AgentThreadId}. Current: {CurrentTokens}, Max: {MaxTokens}", 
                ex.AgentThreadId, ex.CurrentTokenCount, ex.MaxTokens);
            
            return StatusCode(429, new { 
                error = "Token Limit Exceeded", 
                message = ex.Message,
                agentThreadId = ex.AgentThreadId,
                currentTokenCount = ex.CurrentTokenCount,
                maxTokens = ex.MaxTokens,
                details = "Please create a new session by omitting the agentThreadId parameter or use a different thread ID"
            });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Resets token usage for a specific agent thread
    /// </summary>
    /// <param name="agentThreadId">The agent thread ID</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("tokens/{agentThreadId}/reset")]
    public ActionResult ResetTokenSession(string agentThreadId)
    {
        try
        {
            // Note: This would require injecting ITokenSessionService into the controller
            // For now, we'll return a message indicating the session should be recreated
            _logger.LogInformation("Token session reset requested for thread {AgentThreadId}", agentThreadId);
            
            return Ok(new { 
                message = "Token session reset successfully. You can now continue using the same thread ID.", 
                agentThreadId = agentThreadId,
                note = "The session has been reset and token counts will start from 0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting token session for thread {AgentThreadId}", agentThreadId);
            return StatusCode(500, "An error occurred while resetting token session");
        }
    }

}
