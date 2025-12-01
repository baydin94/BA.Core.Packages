using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace Core.Persistence.Mongo.Context;

public class MongoContext
{
    protected readonly IMongoDatabase _mongoDatabase;
    public MongoContext(IOptions<MongoSettings> options)
    {
        MongoSettings settings = options.Value;
        IMongoClient client = new MongoClient(settings.ConnectionString);
        _mongoDatabase = client.GetDatabase(settings.Database);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>()
    {
        string collection = typeof(TDocument).Name.Pluralize();
        return _mongoDatabase.GetCollection<TDocument>(collection);
    }

    // Test - explicit collection name
    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        return _mongoDatabase.GetCollection<TDocument>(collectionName);
    }
}
