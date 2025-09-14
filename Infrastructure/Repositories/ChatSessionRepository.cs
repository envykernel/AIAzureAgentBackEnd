using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.MongoDb;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ChatSessionRepository : BaseRepository<ChatSession>, IChatSessionRepository
{
    public ChatSessionRepository(MongoDbClient mongoDbClient, ILogger<ChatSessionRepository> logger) 
        : base(mongoDbClient, "chat_sessions", logger)
    {
    }

    protected override string GetEntityId(ChatSession entity) => entity.Id;

    public async Task<ChatSession?> GetActiveSessionAsync(string sessionId)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(x => x.Id, sessionId),
                Builders<ChatSession>.Filter.Eq(x => x.IsActive, true)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active chat session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> DeactivateSessionAsync(string sessionId)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.Eq(x => x.Id, sessionId);
            var update = Builders<ChatSession>.Update
                .Set(x => x.IsActive, false)
                .Set(x => x.LastActivityAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating chat session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> UpdateTokenCountAsync(string sessionId, int tokenCount)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.Eq(x => x.Id, sessionId);
            var update = Builders<ChatSession>.Update
                .Set(x => x.TokenCount, tokenCount)
                .Set(x => x.LastActivityAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating token count for session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatSession>> GetSessionsExceedingTokenLimitAsync(int maxTokens)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.Gt(x => x.TokenCount, maxTokens);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions exceeding token limit: {MaxTokens}", maxTokens);
            throw;
        }
    }
}
