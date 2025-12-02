namespace Infrastructure.Persistence.Mongo.Dynamic;

public class Filter
{
    public string? Field { get; set; }
    public string? Value { get; set; }
    public string? Operator { get; set; }
    public string? Logic { get; set; }
    public List<Filter>? Filters { get; set; }

    public Filter()
    {
        Filters = new();
    }

    public Filter(string field, string @operator)
    {
        Field = field;
        Operator = @operator;
    }
}
