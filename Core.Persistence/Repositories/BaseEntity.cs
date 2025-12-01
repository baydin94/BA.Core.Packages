
using Core.Abstractions.Domain;

namespace Core.Persistence.Sql.Repositories;

public class BaseEntity<TKey> : IEntity<TKey>, IHasTimestamps
{
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public TKey Id { get; set; } = default!;

    public BaseEntity() { }

    public BaseEntity(TKey id)
    {
        Id = id;
    }
}
