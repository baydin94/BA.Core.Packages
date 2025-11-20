namespace Core.Persistence.Dynamic;

public class DynamicQuery
{
    public IEnumerable<Sort>? Sorts { get; set; }
    public Filter? Filter { get; set; }
}
