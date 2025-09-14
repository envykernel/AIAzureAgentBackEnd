using Domain.Entities;

namespace Domain.Interfaces;

public interface IChatSessionRepository : IRepository<ChatSession>
{
    Task<ChatSession?> GetActiveSessionAsync(string sessionId);
    Task<bool> DeactivateSessionAsync(string sessionId);
    Task<bool> UpdateTokenCountAsync(string sessionId, int tokenCount);
    Task<IEnumerable<ChatSession>> GetSessionsExceedingTokenLimitAsync(int maxTokens);
}
