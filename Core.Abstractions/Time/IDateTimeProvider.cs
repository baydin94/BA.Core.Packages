namespace Core.Abstractions.Time;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
