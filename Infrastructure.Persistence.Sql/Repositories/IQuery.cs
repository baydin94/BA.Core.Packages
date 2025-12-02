namespace Infrastructure.Persistence.Sql.Repositories;

public interface IQuery<TEntity>
{
    IQueryable<TEntity> Query();
}
