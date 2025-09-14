using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly ILogger<AgentController> _logger;
    private readonly IChatCommandHandler _chatCommandHandler;
    private readonly IChatService _chatService;

    public AgentController(ILogger<AgentController> logger, IChatCommandHandler chatCommandHandler, IChatService chatService)
    {
        _logger = logger;
        _chatCommandHandler = chatCommandHandler;
        _chatService = chatService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            _logger.LogInformation("Chat request received");
            
            var response = await _chatCommandHandler.HandleAsync(request);
            
            _logger.LogInformation("Chat response sent successfully. SessionId: {SessionId}", response.SessionId);
            return Ok(response);
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

    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<ChatSession>> GetSession(string sessionId)
    {
        try
        {
            var session = await _chatService.GetActiveSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound("Session not found or inactive");
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session: {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while retrieving the session");
        }
    }

    [HttpPost("sessions/{sessionId}/deactivate")]
    public async Task<ActionResult> DeactivateSession(string sessionId)
    {
        try
        {
            var success = await _chatService.DeactivateSessionAsync(sessionId);
            if (success)
            {
                return Ok(new { message = "Session deactivated successfully" });
            }
            return NotFound("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating session: {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while deactivating the session");
        }
    }
}
