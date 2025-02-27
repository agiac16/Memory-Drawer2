using MongoDB.Driver;
using NetServer.Models;

namespace NetServer.Services;

public class MongoDBService {
    private readonly IMongoDatabase _database;

    public MongoDBService(IConfiguration configuration, IMongoClient mongoClient)
    {
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "mdtwo";
        _database = mongoClient.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}