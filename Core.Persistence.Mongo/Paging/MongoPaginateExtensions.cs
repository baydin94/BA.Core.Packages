using MongoDB.Driver;

namespace Core.Persistence.Mongo.Paging;

public static class MongoPaginateExtensions
{
    public static async Task<Paginate<TDocument>> ToPaginateAsync<TDocument>(this IFindFluent<TDocument, TDocument> fluent, int index, int size, CancellationToken cancellationToken = default)
    {
        if (size <= 0)
            size = 10;

        int count = (int)await fluent.CountDocumentsAsync(cancellationToken);

        List<TDocument> items = await fluent.Skip(index * size).Limit(size).ToListAsync(cancellationToken);
        Paginate<TDocument> paginate = new Paginate<TDocument>()
        {
            Index = index,
            Size = size,
            Count = count,
            Items = items,
            Pages = count == 0 ? 0 : (int)Math.Ceiling(count / (double)size)
        };
        return paginate;
    }

    public static Paginate<TDocument> ToPaginate<TDocument>(this IFindFluent<TDocument, TDocument> fluent, int index, int size)
    {
        if (size <= 0)
            size = 0;

        int count = (int)fluent.CountDocuments();

        List<TDocument> items = fluent.Skip(index * size).Limit(size).ToList();
        Paginate<TDocument> paginate = new Paginate<TDocument>()
        {
            Index = index,
            Size = size,
            Count = count,
            Items = items,
            Pages = count == 0 ? 0 : (int)Math.Ceiling(count / (double)size)
        };
        return paginate;
    }
}
