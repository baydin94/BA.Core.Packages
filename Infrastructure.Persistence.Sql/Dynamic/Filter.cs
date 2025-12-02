namespace Infrastructure.Persistence.Sql.Dynamic;

public class Filter
{
    public string? Field { get; set; }
    public string? Value { get; set; }
    public string? Operator { get; set; }
    public string? Logic { get; set; }
    public IEnumerable<Filter> Filters { get; set; } = new List<Filter>();

    public Filter()
    {

    }

    public Filter(string field, string @operator)
    {
        Field = field;
        Operator = @operator;
    }
}
