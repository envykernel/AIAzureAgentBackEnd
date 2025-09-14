using Domain.Entities;
using Domain.DTOs;

namespace Domain.Interfaces;

public interface IChatService
{
    
    int EstimateTokenCount(string text);
    Task<AgentResponse> GenerateAgentResponseAsync(Message userMessage, string? agentThreadId = null);
}
