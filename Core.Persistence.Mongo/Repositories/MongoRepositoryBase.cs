using Core.Persistence.Mongo.Context;
using Core.Persistence.Mongo.Dynamic;
using Core.Persistence.Mongo.Paging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Core.Persistence.Mongo.Repositories;

public class MongoRepositoryBase<TDocument> : IMongoRepositoryAsync<TDocument>, IMongoRepository<TDocument>
    where TDocument : BaseDocument

{
    protected readonly MongoContext Context;
    protected readonly IMongoCollection<TDocument> _collection;
    public MongoRepositoryBase(MongoContext context)
    {
        Context = context;
        _collection = Context.GetCollection<TDocument>();
    }
    public bool Any(Expression<Func<TDocument, bool>>? predicate = null, bool withDeleted = false)
    {
        IQueryable<TDocument> queryable = Query(withDeleted);

        if (predicate != null)
            queryable = queryable.Where(predicate);

        return queryable.Any();
    }

    public async Task<bool> AnyAsync(Expression<Func<TDocument, bool>>? predicate = null, bool withDeleted = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TDocument> queryable = Query(withDeleted);

        if (predicate != null)
            queryable = queryable.Where(predicate);

        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<TDocument> CreateAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return document;
    }

    public async Task<ICollection<TDocument>> CreateRangeAsync(ICollection<TDocument> documents, CancellationToken cancellationToken = default)
    {
        await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        return documents;
    }

    public TDocument Get(Expression<Func<TDocument, bool>> predicate, bool withDeleted = false)
    {
        FilterDefinition<TDocument> filter = BuildFilter(predicate, withDeleted);
        return _collection.Find(filter).FirstOrDefault();
    }

    public async Task<TDocument?> GetAsync(Expression<Func<TDocument, bool>> predicate, bool withDeleted = false, CancellationToken cancellationToken = default)
    {
        FilterDefinition<TDocument> filter = BuildFilter(predicate, withDeleted);

        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public Paginate<TDocument> GetList(Expression<Func<TDocument, bool>>? predicate = null, Func<SortDefinitionBuilder<TDocument>, SortDefinition<TDocument>>? orderBy = null, int index = 0, int size = 10, bool withDeleted = false)
    {
        FilterDefinitionBuilder<TDocument> filterBuilder = Builders<TDocument>.Filter;
        FilterDefinition<TDocument> filter = filterBuilder.Empty;

        if (!withDeleted)
            filter = filter & filterBuilder.Eq(x => x.DeletedDate, null);
        if (predicate != null)
            filter = filter & filterBuilder.Where(predicate);

        IFindFluent<TDocument, TDocument> fluent = _collection.Find(filter);

        if (orderBy != null)
        {
            SortDefinitionBuilder<TDocument> sortBuilder = Builders<TDocument>.Sort;
            SortDefinition<TDocument> sortDefinition = orderBy(sortBuilder);
            fluent = fluent.Sort(sortDefinition);
        }

        return fluent.ToPaginate(index, size);
    }

    public async Task<Paginate<TDocument>> GetListAsync(Expression<Func<TDocument, bool>>? predicate = null, Func<SortDefinitionBuilder<TDocument>, SortDefinition<TDocument>>? orderBy = null, int index = 0, int size = 10, bool withDeleted = false, CancellationToken cancellationToken = default)
    {
        FilterDefinitionBuilder<TDocument> filterBuilder = Builders<TDocument>.Filter;
        FilterDefinition<TDocument> filter = filterBuilder.Empty;

        if (!withDeleted)
            filter = filter & filterBuilder.Eq(x => x.DeletedDate, null);
        if (predicate != null)
            filter = filter & filterBuilder.Where(predicate);

        IFindFluent<TDocument, TDocument> fluent = _collection.Find(filter);

        if (orderBy != null)
        {
            SortDefinitionBuilder<TDocument> sortBuilder = Builders<TDocument>.Sort;
            SortDefinition<TDocument> sortDefinition = orderBy(sortBuilder);
            fluent = fluent.Sort(sortDefinition);
        }

        return await fluent.ToPaginateAsync(index, size, cancellationToken);
    }

    public Paginate<TDocument> GetListByDynamic(DynamicQuery dynamicQuery, Expression<Func<TDocument, bool>>? predicate = null, int index = 0, int size = 10, bool withDeleted = false)
    {
        IFindFluent<TDocument, TDocument> fluent = _collection.ToDynamic(dynamicQuery);

        if (!withDeleted)
        {
            FilterDefinition<TDocument> withDeletedFilter = Builders<TDocument>.Filter.Eq(x => x.DeletedDate, null);
            fluent = _collection.Find(Builders<TDocument>.Filter.And(fluent.Filter, withDeletedFilter));
        }

        if (predicate != null)
        {
            FilterDefinition<TDocument> predicateFilter = Builders<TDocument>.Filter.Where(predicate);
            fluent = _collection.Find(Builders<TDocument>.Filter.And(fluent.Filter, predicateFilter));
        }

        return fluent.ToPaginate(index, size);
    }

    public async Task<Paginate<TDocument>> GetListByDynamicAsync(DynamicQuery dynamicQuery, Expression<Func<TDocument, bool>>? predicate = null, int index = 0, int size = 10, bool withDeleted = false, CancellationToken cancellationToken = default)
    {
        IFindFluent<TDocument, TDocument> fluent = _collection.ToDynamic(dynamicQuery);

        if (!withDeleted)
        {
            FilterDefinition<TDocument> withDeletedFilter = Builders<TDocument>.Filter.Eq(x => x.DeletedDate, null);
            fluent = _collection.Find(Builders<TDocument>.Filter.And(fluent.Filter, withDeletedFilter));
        }

        if (predicate != null)
        {
            FilterDefinition<TDocument> predicateFilter = Builders<TDocument>.Filter.Where(predicate);
            fluent = _collection.Find(Builders<TDocument>.Filter.And(fluent.Filter, predicateFilter));
        }

        return await fluent.ToPaginateAsync(index, size, cancellationToken);
    }

    public IQueryable<TDocument> Query(bool withDeleted = false)
    {
        IQueryable<TDocument> queryable = _collection.AsQueryable();

        if (!withDeleted)
            queryable = queryable.Where(x => x.DeletedDate == null);
        return queryable;
    }

    public async Task<TDocument> RemoveAsync(TDocument document, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (permanent)
        {
            await _collection.DeleteOneAsync(x => x.Id == document.Id, cancellationToken);
            return document;
        }

        document.DeletedDate = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == document.Id, document, cancellationToken: cancellationToken);
        return document;
    }

    public async Task<ICollection<TDocument>> RemoveRangeAsync(ICollection<TDocument> documents, bool permanent = false, CancellationToken cancellationToken = default)
    {
        List<string> ids = documents.Select(d => d.Id).ToList();

        if (permanent)
        {
            await _collection.DeleteManyAsync(Builders<TDocument>.Filter.In(x => x.Id, ids));
            return documents;
        }

        await _collection.UpdateManyAsync(Builders<TDocument>.Filter.In(x => x.Id, ids), Builders<TDocument>.Update.Set(x => x.DeletedDate, DateTime.UtcNow), cancellationToken: cancellationToken);

        return documents;
    }

    public async Task<TDocument> UpdateAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        document.UpdatedDate = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(filter: x => x.Id == document.Id, replacement: document, cancellationToken: cancellationToken);
        return document;
    }

    public async Task<ICollection<TDocument>> UpdateRangeAsync(ICollection<TDocument> documents, CancellationToken cancellationToken = default)
    {
        foreach (TDocument document in documents)
            document.UpdatedDate = DateTime.UtcNow;

        List<WriteModel<TDocument>> operations = new List<WriteModel<TDocument>>();

        foreach (TDocument document in documents)
        {
            FilterDefinition<TDocument> filterDefinition = Builders<TDocument>.Filter.Eq(x => x.Id, document.Id);
            ReplaceOneModel<TDocument> model = new ReplaceOneModel<TDocument>(filterDefinition, document);
            operations.Add(model);
        }

        await _collection.BulkWriteAsync(operations, cancellationToken: cancellationToken);
        return documents;
    }

    protected FilterDefinition<TDocument> BuildFilter(Expression<Func<TDocument, bool>> predicate, bool withDeleted)
    {
        FilterDefinitionBuilder<TDocument> filterBuilder = Builders<TDocument>.Filter;
        FilterDefinition<TDocument> filterDefinition = filterBuilder.Empty;

        if (!withDeleted)
            filterDefinition &= filterBuilder.Eq(x => x.DeletedDate, null);

        filterDefinition &= filterBuilder.Where(predicate);
        return filterDefinition;
    }
}
