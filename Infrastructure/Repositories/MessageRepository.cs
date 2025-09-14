using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.MongoDb;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Infrastructure.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(MongoDbClient mongoDbClient, ILogger<MessageRepository> logger) 
        : base(mongoDbClient, "messages", logger)
    {
    }

    protected override string GetEntityId(Message entity) => entity.Id;

    public async Task<IEnumerable<Message>> GetBySessionIdAsync(string sessionId)
    {
        try
        {
            var filter = Builders<Message>.Filter.Eq(x => x.SessionId, sessionId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages by session id: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetBySessionIdOrderedAsync(string sessionId, int limit = 0)
    {
        try
        {
            var filter = Builders<Message>.Filter.Eq(x => x.SessionId, sessionId);
            var sort = Builders<Message>.Sort.Ascending(x => x.CreatedAt);
            
            var query = _collection.Find(filter).Sort(sort);
            
            if (limit > 0)
            {
                query = query.Limit(limit);
            }
            
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ordered messages by session id: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<int> GetTokenCountBySessionIdAsync(string sessionId)
    {
        try
        {
            var filter = Builders<Message>.Filter.Eq(x => x.SessionId, sessionId);
            var pipeline = new EmptyPipelineDefinition<Message>()
                .Match(filter)
                .Group(new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalTokens", new BsonDocument("$sum", "$TokenCount") }
                });
            
            var result = await _collection.Aggregate(pipeline).FirstOrDefaultAsync();
            return result?["totalTokens"]?.AsInt32 ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token count by session id: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> DeleteMessagesBySessionIdAsync(string sessionId)
    {
        try
        {
            var filter = Builders<Message>.Filter.Eq(x => x.SessionId, sessionId);
            var result = await _collection.DeleteManyAsync(filter);
            
            _logger.LogInformation("Deleted {Count} messages for session: {SessionId}", result.DeletedCount, sessionId);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting messages by session id: {SessionId}", sessionId);
            throw;
        }
    }
}
