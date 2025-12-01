namespace Core.Abstractions.Domain;

public interface IHasKey<TKey>
{
    TKey Id { get; set; }
}
