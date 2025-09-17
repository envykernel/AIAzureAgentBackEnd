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
            
            // Check if the response contains an error (non-token related errors)
            if (response.IsError && response.ErrorType != "TokenLimitExceeded")
            {
                _logger.LogError("Error in chat response: {ErrorType} - {ErrorMessage}", response.ErrorType, response.ErrorMessage);
                return StatusCode(500, new { 
                    error = response.ErrorType, 
                    message = response.ErrorMessage,
                    details = "Please try again or contact support if the issue persists"
                });
            }
            
            // Check if session is closed due to token limit exceeded
            if (response.Session.IsSessionClosed)
            {
                _logger.LogWarning("Session closed due to token limit exceeded for thread {AgentThreadId}. Token usage: {TokenUsagePercentage:F1}%", 
                    response.Session.AgentThreadId, response.TokenUsage.TokenUsagePercentage);
            }
            
            _logger.LogInformation("Chat response sent successfully. AgentThreadId: {AgentThreadId}", response.Session.AgentThreadId);
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
    /// Deletes a specific agent thread
    /// </summary>
    /// <param name="agentThreadId">The agent thread ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("threads/{agentThreadId}")]
    public async Task<ActionResult> DeleteThread(string agentThreadId)
    {
        try
        {
            _logger.LogInformation("Thread deletion requested for thread {AgentThreadId}", agentThreadId);
            
            // Call the handler to delete the thread and reset the session
            var success = await _chatCommandHandler.DeleteThreadAsync(agentThreadId);
            
            if (success)
            {
                _logger.LogInformation("Thread {AgentThreadId} deleted successfully", agentThreadId);
                return Ok(new { 
                    message = "Thread deleted successfully. The Azure AI Agent thread has been permanently deleted and you can now create a new conversation.", 
                    agentThreadId = agentThreadId,
                    note = "The thread has been deleted and token counts will start from 0 for new conversations"
                });
            }
            else
            {
                _logger.LogWarning("Failed to delete thread {AgentThreadId}", agentThreadId);
                return StatusCode(500, new {
                    error = "Thread Deletion Failed",
                    message = "Failed to delete the thread. Please try again or contact support.",
                    agentThreadId = agentThreadId
                });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid thread ID format: {AgentThreadId}", agentThreadId);
            return BadRequest(new { 
                error = "Invalid Thread ID", 
                message = ex.Message,
                agentThreadId = agentThreadId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting thread {AgentThreadId}", agentThreadId);
            return StatusCode(500, new {
                error = "Internal Error",
                message = "An error occurred while deleting the thread",
                agentThreadId = agentThreadId
            });
        }
    }

}
