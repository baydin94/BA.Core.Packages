namespace Core.Persistence.Sql.Dynamic;

public class Sort
{
    public string Field { get; set; }
    public string Direction { get; set; }

    public Sort(string field, string direction)
    {
        Field = field;
        Direction = direction;
    }
}
