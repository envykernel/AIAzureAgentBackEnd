using Domain.DTOs;

namespace Domain.Interfaces;

public interface IChatService
{
    int EstimateTokenCount(string text);
    Task<AgentResponse> GenerateAgentResponseAsync(string userMessageContent, string? agentThreadId = null);
}
