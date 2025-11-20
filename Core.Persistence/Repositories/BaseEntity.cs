
namespace Core.Persistence.Repositories;

public class BaseEntity<TEntityId> : IEntityTimestamps
{
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public TEntityId Id { get; set; } = default!;

    public BaseEntity() { }

    public BaseEntity(TEntityId id)
    {
        Id = id;
    }
}
