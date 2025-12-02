using Infrastructure.Persistence.Mongo.Dynamic;
using Infrastructure.Persistence.Mongo.Paging;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Mongo.Repositories;

public interface IMongoRepositoryAsync<TDocument> : IQuery<TDocument>
    where TDocument : BaseDocument
{
    Task<TDocument?> GetAsync(
        Expression<Func<TDocument, bool>> predicate,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    );

    Task<Paginate<TDocument>> GetListAsync(
        Expression<Func<TDocument, bool>>? predicate = null,
        Func<SortDefinitionBuilder<TDocument>, SortDefinition<TDocument>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    );

    Task<Paginate<TDocument>> GetListByDynamicAsync(
        DynamicQuery dynamicQuery,
        Expression<Func<TDocument, bool>>? predicate = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync(
        Expression<Func<TDocument, bool>>? predicate = null,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    );

    Task<TDocument> CreateAsync(TDocument document, CancellationToken cancellationToken = default);
    Task<ICollection<TDocument>> CreateRangeAsync(ICollection<TDocument> documents, CancellationToken cancellationToken = default);
    Task<TDocument> UpdateAsync(TDocument document, CancellationToken cancellationToken = default);
    Task<ICollection<TDocument>> UpdateRangeAsync(ICollection<TDocument> documents, CancellationToken cancellationToken = default);
    Task<TDocument> RemoveAsync(TDocument document, bool permanent = false, CancellationToken cancellationToken = default);
    Task<ICollection<TDocument>> RemoveRangeAsync(ICollection<TDocument> documents, bool permanent = false, CancellationToken cancellationToken = default);

}
