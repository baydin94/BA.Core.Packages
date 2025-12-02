using Infrastructure.Persistence.Sql.Repositories;
using Core.Tests.Persistence.Sql.TestEntities;

namespace Core.Tests.Persistence.Sql.TestRepositories;

public class TestEntityRepository : EfRepositoryBase<TestEntity, Guid, TestDbContext>
{
    public TestEntityRepository(TestDbContext context) : base(context)
    {
    }
}
