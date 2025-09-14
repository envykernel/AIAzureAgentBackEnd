using Domain.Entities;

namespace Domain.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<IEnumerable<Message>> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<Message>> GetBySessionIdOrderedAsync(string sessionId, int limit = 0);
    Task<int> GetTokenCountBySessionIdAsync(string sessionId);
    Task<bool> DeleteMessagesBySessionIdAsync(string sessionId);
}
