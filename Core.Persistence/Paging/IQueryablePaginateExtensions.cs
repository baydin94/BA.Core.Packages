using Microsoft.EntityFrameworkCore;

namespace Core.Persistence.Sql.Paging;

public static class IQueryablePaginateExtensions
{
    public static async Task<Paginate<TEntity>> ToPaginateAsync<TEntity>(this IQueryable<TEntity> source, int index, int size, CancellationToken cancellationToken = default)
    {
        if (size <= 0)
            size = 10;

        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<TEntity> items = await source.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);

        Paginate<TEntity> paginate = new()
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (count == 0) ? 0 : (int)Math.Ceiling(count / (double)size),
        };
        return paginate;
    }

    public static Paginate<TEntity> ToPaginate<TEntity>(this IQueryable<TEntity> source, int index, int size)
    {
        if (size <= 0)
            size = 10;
        int count = source.Count();
        List<TEntity> items = source.Skip(index * size).Take(size).ToList();
        Paginate<TEntity> paginate = new()
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (count == 0) ? 0 : (int)Math.Ceiling(count / (double)size),
        };
        return paginate;
    }
}
