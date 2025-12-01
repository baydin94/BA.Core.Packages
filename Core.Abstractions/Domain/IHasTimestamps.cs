namespace Core.Abstractions.Domain;

public interface IHasTimestamps
{
    DateTime CreatedDate { get; set; }
    DateTime? UpdatedDate { get; set; }
    DateTime? DeletedDate { get; set; }
}
