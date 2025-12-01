using Core.Persistence.Mongo.Context;
using Core.Persistence.Mongo.Dynamic;
using Core.Persistence.Mongo.Paging;
using Core.Persistence.Mongo.Repositories;
using Core.Tests.Persistence.Mongo.Fixtures;
using Core.Tests.Persistence.Mongo.TestDocuments;
using Core.Tests.Persistence.Mongo.TestRepositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Core.Tests.Persistence.Mongo.Repositories;

public class MongoRepositoryBaseTests : IClassFixture<MongoTestFixture>
{
    private readonly MongoTestFixture _fixture;
    private readonly MongoRepositoryBase<TestDocument> _mongoRepositoryBase;
    private string _collectionName;
    public MongoRepositoryBaseTests(MongoTestFixture fixture)
    {
        _fixture = fixture;

        var settings = Options.Create(new MongoSettings
        {
            ConnectionString = fixture.Runner.ConnectionString,
            Database = "TestDb"
        });

        _collectionName = "Test_" + Guid.NewGuid().ToString("N");

        var context = new MongoContext(settings);
        _mongoRepositoryBase = new TestRepository(context, _collectionName);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task CreateAsync_Should_Insert_Document()
    {
        TestDocument document = new TestDocument { Name = "Test123" };

        await _mongoRepositoryBase.CreateAsync(document);

        var result = await _mongoRepositoryBase.GetAsync(
            predicate: x => x.Id == document.Id
            );
        result.Should().NotBeNull();
        result.Name.Should().Be("Test123");
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task UpdateAsync_Should_Replace_Document()
    {
        //Arrange
        TestDocument document = new TestDocument { Name = "OldName" };
        await _mongoRepositoryBase.CreateAsync(document);
        var originalId = document.Id;

        document.Name = "NewName";

        //Act
        await _mongoRepositoryBase.UpdateAsync(document);

        //Assert
        var result = await _mongoRepositoryBase.GetAsync(x => x.Id == originalId);
        result.Should().NotBeNull();
        result.Id.Should().Be(originalId);
        result.Name.Should().Be("NewName");
    }

    [Trait("Category", "Remove")]
    [Fact]
    public async Task RemoveAsync_Should_WithPermanent_Delete_Document()
    {
        //Arrange
        TestDocument testDocument = new TestDocument { Name = "DeleteCategory" };
        await _mongoRepositoryBase.CreateAsync(testDocument);

        string originalId = testDocument.Id;

        //Act
        await _mongoRepositoryBase.RemoveAsync(
            document: testDocument,
            permanent: true
            );

        //Assert
        TestDocument? result = await _mongoRepositoryBase.GetAsync(x => x.Id == originalId);
        result.Should().BeNull();
    }

    [Trait("Category", "Remove")]
    [Fact]
    public async Task RemoveAsyn_Should_SoftDelete_Document()
    {
        //arrange
        TestDocument testDocument = new TestDocument { Name = "SoftDelete" };
        await _mongoRepositoryBase.CreateAsync(testDocument);
        string id = testDocument.Id;

        //act
        await _mongoRepositoryBase.RemoveAsync(document: testDocument, permanent: false);

        //assert
        TestDocument? result = await _mongoRepositoryBase.GetAsync(
            predicate: x => x.Id == id,
            withDeleted: true
            );
        result.Should().NotBeNull();
        result.Name.Should().Be("SoftDelete");
        result.DeletedDate.Should().NotBeNull();
    }

    [Trait("Category", "Any")]
    [Fact]
    public async Task Should_Return_True_When_Document_Exists()
    {
        //Arrange
        TestDocument testDocument = new TestDocument { Name = "any" };
        await _mongoRepositoryBase.CreateAsync(testDocument);
        string id = testDocument.Id;

        //Act
        bool result = await _mongoRepositoryBase.AnyAsync();

        //Assert
        result.Should().BeTrue();
    }

    [Trait("Category", "CreateRange")]
    [Fact]
    public async Task CreateRangeAsync_Should_Insert_All_Documents()
    {
        //arrange
        List<TestDocument> documents = new List<TestDocument>
        {
            new TestDocument {Name = "category1"},
            new TestDocument {Name = "category2"},
            new TestDocument {Name = "category3"}
        };

        //act
        await _mongoRepositoryBase.CreateRangeAsync(documents);

        Paginate<TestDocument> results = await _mongoRepositoryBase.GetListAsync();

        //assert
        results.Should().NotBeNull();
        results.Items.Should().HaveCount(3);
        foreach (TestDocument result in results.Items)
        {
            result.CreatedDate.Should().NotBe(default);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Trait("Category", "Dynamic")]
    [Fact]
    public async Task GetListByDynamicAsync_Should_Filter_By_Contains_And_Sort_Ascending()
    {
        // Arrange
        await _mongoRepositoryBase.CreateRangeAsync(new List<TestDocument>
        {
            new() { Name = "cat food" },
            new() { Name = "dog food" },
            new() { Name = "category" },
            new() { Name = "apple" }
        });

        DynamicQuery query = new()
        {
            Filter = new Filter
            {
                Field = "Name",
                Operator = "contains",
                Value = "cat"
            },
            Sorts = new List<Sort>
            {
                new Sort
                {
                    Field = "Name",
                    Direction = "asc"
                }
            }
        };

        // Act
        Paginate<TestDocument> result = await _mongoRepositoryBase.GetListByDynamicAsync(query);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("cat food");
        result.Items[1].Name.Should().Be("category");
    }

    [Trait("Category", "Dynamic")]
    [Fact]
    public async Task GetListByDynamicAsync_Should_Filter_With_And_Logic()
    {
        await _mongoRepositoryBase.CreateRangeAsync(new List<TestDocument>
        {
            new() { Name = "cat food", Status = "Active" },
            new() { Name = "cat litter", Status = "Inactive" },
            new() { Name = "dog food", Status = "Active" }
        });

        DynamicQuery query = new()
        {
            Filter = new Filter
            {
                Logic = "and",
                Filters = new List<Filter>
                {
                    new()
                    {
                        Field = "Name",
                        Operator = "contains",
                        Value = "cat"
                    },
                    new()
                    {
                        Field = "Status",
                        Operator = "eq",
                        Value = "Active"
                    }
                }
            }
        };

        //Act
        Paginate<TestDocument> results = await _mongoRepositoryBase.GetListByDynamicAsync(query);

        //Assert
        results.Items.Should().HaveCount(1);
        results.Items.First().Name.Should().Be("cat food");
    }

    [Trait("Category", "Dynamic")]
    [Fact]
    public async Task GetListByDynamicAsync_Should_Filter_With_Or_Logic()
    {
        // Arrange
        await _mongoRepositoryBase.CreateRangeAsync(new[]
        {
            new TestDocument { Name = "cat" },
            new TestDocument { Name = "dog" },
            new TestDocument { Name = "apple" }
        });

        DynamicQuery query = new()
        {
            Filter = new Filter
            {
                Logic = "or",
                Filters = new List<Filter>
                {
                    new() { Field = "Name", Operator = "eq", Value = "cat" },
                    new() { Field = "Name", Operator = "eq", Value = "dog" }
                }
            }
        };

        // Act
        var result = await _mongoRepositoryBase.GetListByDynamicAsync(query);

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Trait("Category", "Dynamic")]
    [Fact]
    public async Task GetListByDynamicAsync_Should_Throw_When_Operator_Invalid()
    {
        DynamicQuery query = new()
        {
            Filter = new Filter
            {
                Field = "Name",
                Operator = "invalid_op",
                Value = "cat"
            }
        };

        Func<Task> act = async () => await _mongoRepositoryBase.GetListByDynamicAsync(query);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid Operator");
    }

    [Trait("Category", "Dynamic")]
    [Fact]
    public async Task DynamicQuery_DoesNotContain_Should_Exclude_Matching_Items()
    {
        //Arrange
        var docs = new List<TestDocument>
        {
            new TestDocument {Name = "Apple"},
            new TestDocument {Name = "Banana"},
            new TestDocument {Name = "Grapes"}
        };
        await _mongoRepositoryBase.CreateRangeAsync(docs);

        DynamicQuery query = new()
        {
            Filter = new Filter
            {
                Field = "Name",
                Operator = "doesnotcontain",
                Value = "ap"
            }
        };

        //Act
        Paginate<TestDocument> result = await _mongoRepositoryBase.GetListByDynamicAsync(query);

        //Assert
        result.Items.Should().ContainSingle(x => x.Name == "Banana");
    }
}
