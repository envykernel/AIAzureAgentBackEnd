using Domain.Entities;

namespace Domain.Interfaces;

public interface IConversationSummaryRepository : IRepository<ConversationSummary>
{
    Task<ConversationSummary?> GetBySessionIdAsync(string sessionId);
    Task<bool> UpdateSummaryAsync(string sessionId, string summary, int tokenCount);
    Task<bool> DeleteBySessionIdAsync(string sessionId);
}
