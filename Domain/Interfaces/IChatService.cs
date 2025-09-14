using Domain.Entities;
using Domain.DTOs;

namespace Domain.Interfaces;

public interface IChatService
{
    Task<ChatSession> CreateSessionAsync();
    Task<(Message message, ChatSession updatedSession)> SendMessageAsync(string sessionId, string content, MessageRole role, int tokenCount);
    Task<IEnumerable<Message>> GetConversationAsync(string sessionId, int maxTokens = 4000);
    int EstimateTokenCount(string text);
    Task<ChatSession?> GetActiveSessionAsync(string sessionId);
    Task<bool> DeactivateSessionAsync(string sessionId);
    Task<AgentResponse> GenerateAgentResponseAsync(IEnumerable<Message> conversation, string? agentThreadId = null);
}
