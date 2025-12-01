using Core.Tests.Persistence.Sql.Fixtures;
using Core.Tests.Persistence.Sql.TestEntities;
using Core.Tests.Persistence.Sql.TestRepositories;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Persistence.Sql.Repositories;

public class EfRepositoryTests : IClassFixture<EfSqlTestFixture>
{
    private readonly EfSqlTestFixture _fixture;
    private readonly TestEntityRepository _testEntityRepository;
    public EfRepositoryTests(EfSqlTestFixture fixture)
    {
        _fixture = fixture;
        _testEntityRepository = new TestEntityRepository(fixture.Context);
    }

    [Trait("TestEntity","Create")]
    [Fact]
    public async Task CreateAsync_Should_Insert_Record()
    {
        // Arrange
        TestEntity testEntity = new TestEntity { Name = "TestEntity" };

        // Act
        await _testEntityRepository.CreateAsync(testEntity);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        TestEntity? result = await _testEntityRepository.GetAsync(x => x.Id == testEntity.Id);
        result.Should().NotBeNull();
        result.Name.Should().Be("TestEntity");

    }
}
