using Core.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Sql.Repositories;

public class EfUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    protected readonly TContext Context;

    public EfUnitOfWork(TContext context)
    {
        Context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }
}
