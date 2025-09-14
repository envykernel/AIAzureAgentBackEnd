using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.MongoDb;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ConversationSummaryRepository : BaseRepository<ConversationSummary>, IConversationSummaryRepository
{
    public ConversationSummaryRepository(MongoDbClient mongoDbClient, ILogger<ConversationSummaryRepository> logger) 
        : base(mongoDbClient, "conversation_summaries", logger)
    {
    }

    protected override string GetEntityId(ConversationSummary entity) => entity.Id;

    public async Task<ConversationSummary?> GetBySessionIdAsync(string sessionId)
    {
        try
        {
            var filter = Builders<ConversationSummary>.Filter.Eq(x => x.SessionId, sessionId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation summary by session id: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> UpdateSummaryAsync(string sessionId, string summary, int tokenCount)
    {
        try
        {
            var filter = Builders<ConversationSummary>.Filter.Eq(x => x.SessionId, sessionId);
            var update = Builders<ConversationSummary>.Update
                .Set(x => x.Summary, summary)
                .Set(x => x.TokenCount, tokenCount)
                .Set(x => x.LastUpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation summary for session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> DeleteBySessionIdAsync(string sessionId)
    {
        try
        {
            var filter = Builders<ConversationSummary>.Filter.Eq(x => x.SessionId, sessionId);
            var result = await _collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("Conversation summary deleted for session: {SessionId}", sessionId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation summary by session id: {SessionId}", sessionId);
            throw;
        }
    }
}
