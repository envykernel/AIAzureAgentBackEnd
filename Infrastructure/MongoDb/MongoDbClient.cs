using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.MongoDb;

public class MongoDbClient
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbClient> _logger;

    public MongoDbClient(IConfiguration configuration, ILogger<MongoDbClient> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:password123@localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "agentsdb";
        
        try
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            
            _logger.LogInformation("MongoDB client initialized successfully. Database: {DatabaseName}", databaseName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB client");
            throw;
        }
    }

    public IMongoDatabase Database => _database;
    
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}
