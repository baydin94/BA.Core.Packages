using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Persistence.Sql.TestEntities;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}
