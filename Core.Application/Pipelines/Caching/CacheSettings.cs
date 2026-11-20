namespace Core.Application.Pipelines.Caching;

public class CacheSettings
{
    public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);

    public int SlidingExpiration { get; set; }
}
