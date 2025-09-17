using Domain.DTOs;

namespace Domain.Interfaces;

public interface IChatService
{
    Task<AgentResponse> GenerateAgentResponseAsync(string userMessageContent, string? agentThreadId = null);
    Task<bool> DeleteThreadAsync(string agentThreadId);
}
