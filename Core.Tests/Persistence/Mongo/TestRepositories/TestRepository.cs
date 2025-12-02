
using Core.Tests.Persistence.Mongo.TestDocuments;
using Infrastructure.Persistence.Mongo.Context;
using Infrastructure.Persistence.Mongo.Repositories;

namespace Core.Tests.Persistence.Mongo.TestRepositories;

public class TestRepository : MongoRepositoryBase<TestDocument>
{
    public TestRepository(MongoContext context) : base(context) { }
    public TestRepository(MongoContext context, string collectionName) : base(context,collectionName) { }
}
