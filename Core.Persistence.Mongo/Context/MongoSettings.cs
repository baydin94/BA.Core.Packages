namespace Core.Persistence.Mongo.Context;

public class MongoSettings
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
}
