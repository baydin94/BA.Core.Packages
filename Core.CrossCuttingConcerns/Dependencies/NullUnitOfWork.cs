using Core.Abstractions.Dependencies;

namespace Core.CrossCuttingConcerns.Dependencies;

public class NullUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}
