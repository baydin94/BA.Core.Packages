using Core.Abstractions.Domain;
using Infrastructure.Persistence.Sql.Dynamic;
using Infrastructure.Persistence.Sql.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Sql.Repositories;

public class EfRepositoryBase<TEntity, TEntityId, TContext> : IRepositoryAsync<TEntity, TEntityId>, IRepository<TEntity, TEntityId>
    where TEntity : BaseEntity<TEntityId>
    where TContext : DbContext
{
    protected readonly TContext Context;
    public EfRepositoryBase(TContext context)
    {
        Context = context;
    }

    public bool Any(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        return queryable.Any();
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        return queryable.AnyAsync(cancellationToken);

    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedDate = DateTime.UtcNow;
        await Context.AddAsync(entity);
        return entity;
    }

    public async Task<ICollection<TEntity>> CreateRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (TEntity entity in entities)
            entity.CreatedDate = DateTime.UtcNow;
        await Context.AddRangeAsync(entities);
        return entities;
    }

    public TEntity Get(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (include != null)
            queryable = include(queryable);

        return queryable.FirstOrDefault(predicate)!;
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (include != null)
            queryable = include(queryable);

        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Paginate<TEntity> GetList(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (include != null)
            queryable = include(queryable);
        if (orderBy != null)
            queryable = orderBy(queryable);
        return queryable.ToPaginate(index, size);
    }

    public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (include != null)
            queryable = include(queryable);
        if (orderBy != null)
            queryable = orderBy(queryable);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public Paginate<TEntity> GetListByDynamic(DynamicQuery dynamicQuery, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query().ToDynamic(dynamicQuery);
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (include != null)
            queryable = include(queryable);
        return queryable.ToPaginate(index, size);
    }

    public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query().ToDynamic(dynamic);
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (include != null)
            queryable = include(queryable);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public IQueryable<TEntity> Query() => Context.Set<TEntity>();

    public async Task<TEntity> RemoveAsync(TEntity entity, bool permanent = false)
    {
        await SetEntityAsSoftDeletedAsync(entity, permanent);
        return entity;
    }

    protected async Task SetEntityAsSoftDeletedAsync(TEntity entity, bool permanent)
    {
        if (permanent)
        {
            Context.Remove(entity);
            return;
        }

        EnsureNoOneToOne(entity);

        HashSet<object> visited = new HashSet<object>();
        await SoftDeleteRecursiveAsync(entity, visited);
        Context.Update(entity);
    }

    private async Task SoftDeleteRecursiveAsync(IHasTimestamps entity, HashSet<object> visited)
    {
        if (entity.DeletedDate.HasValue)
            return;
        if (visited.Contains(entity))
            return;

        visited.Add(entity);

        entity.DeletedDate = DateTime.UtcNow;

        IReadOnlyList<INavigation> navigations = Context
            .Entry(entity)
            .Metadata
            .GetNavigations()
            .Where(n => n.ForeignKey.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade && !n.TargetEntityType.IsOwned()).ToList();

        foreach (INavigation navigation in navigations)
        {
            EntityEntry entry = Context.Entry(entity);

            if (navigation.IsCollection)
            {
                CollectionEntry collectionEntry = entry.Collection(navigation.Name);
                if (!collectionEntry.IsLoaded)
                    await collectionEntry.LoadAsync();
                IEnumerable? value = collectionEntry.CurrentValue;
                if (value == null)
                    continue;
                foreach (object child in value)
                {
                    if (child is IHasTimestamps childEntity)
                        await SoftDeleteRecursiveAsync(childEntity, visited);
                }
            }
            else
            {
                ReferenceEntry referenceEntry = entry.Reference(navigation.Name);
                if (!referenceEntry.IsLoaded)
                    await referenceEntry.LoadAsync();
                object? value = referenceEntry.CurrentValue;
                if (value is IHasTimestamps entityVale)
                    await SoftDeleteRecursiveAsync(entityVale, visited);
            }
        }
    }

    private void EnsureNoOneToOne(TEntity entity)
    {
        var oneToOneExists = Context
            .Entry(entity)
            .Metadata
            .GetForeignKeys()
            .Any(fk => fk.IsUnique && !fk.IsOwnership);
        if (oneToOneExists)
            throw new InvalidOperationException("Entity has a one-to-one relation. Soft-delete on one-to-one entities is unsafe.");
    }

    public async Task<ICollection<TEntity>> RemoveRangeAsync(ICollection<TEntity> entities, bool permanent = false)
    {
        await SetEntityAsSoftDeletedAsync(entities, permanent);
        return entities;
    }

    protected async Task SetEntityAsSoftDeletedAsync(ICollection<TEntity> entities, bool permanent)
    {
        foreach (TEntity entity in entities)
            await SetEntityAsSoftDeletedAsync(entity, permanent);
    }

    public TEntity Update(TEntity entity)
    {
        entity.UpdatedDate = DateTime.UtcNow;
        Context.Update(entity);
        return entity;
    }

    public ICollection<TEntity> UpdateRange(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
            entity.UpdatedDate = DateTime.UtcNow;
        Context.UpdateRange(entities);
        return entities;
    }
}
