namespace Core.Persistence.Mongo.Paging;

public class Paginate<TDocument>
{
    public int Size { get; set; }
    public int Index { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IList<TDocument>? Items { get; set; }
    public bool HasPrevious => Index > 0;
    public bool HasNext => Index + 1 < Pages;
}
