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
        string collection = typeof(TDocument).Name;
        return _mongoDatabase.GetCollection<TDocument>(collection);
    }
}
