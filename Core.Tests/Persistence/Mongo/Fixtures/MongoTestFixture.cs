using Mongo2Go;
using MongoDB.Driver;

namespace Core.Tests.Persistence.Mongo.Fixtures;

public class MongoTestFixture : IDisposable
{
    public MongoDbRunner Runner { get; }
    public IMongoDatabase Database { get; }

    public MongoTestFixture()
    {
        Runner = MongoDbRunner.Start();   // In-memory MongoDB başlatır
        var client = new MongoClient(Runner.ConnectionString);
        Database = client.GetDatabase("TestDb");
    }
    public void Dispose()
    {
        Runner.Dispose(); //Test bitince mongoyu kapatır.
    }
}
