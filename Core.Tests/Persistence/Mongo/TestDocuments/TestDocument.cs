using Core.Persistence.Mongo.Repositories;

namespace Core.Tests.Persistence.Mongo.TestDocuments;

public class TestDocument : BaseDocument
{
    public string? Name { get; set; }
    public string? Status { get; set; }
}
