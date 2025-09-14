using Domain.Interfaces;
using Infrastructure.MongoDb;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly ILogger<BaseRepository<T>> _logger;

    protected BaseRepository(MongoDbClient mongoDbClient, string collectionName, ILogger<BaseRepository<T>> logger)
    {
        _collection = mongoDbClient.GetCollection<T>(collectionName);
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by id: {Id}", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetByFilterAsync(Func<T, bool> filter)
    {
        try
        {
            var allEntities = await GetAllAsync();
            return allEntities.Where(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities by filter");
            throw;
        }
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity);
            _logger.LogInformation("Entity created successfully");
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity");
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            var id = GetEntityId(entity);
            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.ReplaceOneAsync(filter, entity);
            
            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Entity updated successfully. Id: {Id}", id);
                return entity;
            }
            
            throw new InvalidOperationException($"Entity with id {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity");
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("Entity deleted successfully. Id: {Id}", id);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity. Id: {Id}", id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if entity exists. Id: {Id}", id);
            throw;
        }
    }

    protected abstract string GetEntityId(T entity);
}
