using Core.Tests.Persistence.Sql.TestEntities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Persistence.Sql.Fixtures;

public class EfSqlTestFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public DbContextOptions<TestDbContext> Options { get; }
    public TestDbContext Context { get; }

    public EfSqlTestFixture()
    {
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        Options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(Connection)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)   // Update testleri için
            .Options;

        Context = new TestDbContext(Options);

        Context.Database.EnsureCreated();
    }
    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }


}
