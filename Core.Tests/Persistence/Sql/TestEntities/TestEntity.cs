using Infrastructure.Persistence.Sql.Repositories;

namespace Core.Tests.Persistence.Sql.TestEntities;

public class TestEntity : BaseEntity<Guid>
{
    public string Name { get; set; } = "";
}
