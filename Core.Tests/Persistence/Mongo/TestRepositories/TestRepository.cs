using Core.Persistence.Mongo.Context;
using Core.Persistence.Mongo.Repositories;
using Core.Tests.Persistence.Mongo.TestDocuments;

namespace Core.Tests.Persistence.Mongo.TestRepositories;

public class TestRepository : MongoRepositoryBase<TestDocument>
{
    public TestRepository(MongoContext context) : base(context) { }
    public TestRepository(MongoContext context, string collectionName) : base(context,collectionName) { }
}
