namespace Core.Abstractions.Applications.Caching;

/// <summary>
/// Represents the result of a cache read operation.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public class CacheReadResult<T>
{
    /// <summary>
    /// Indicates whether the requested entry was found in the cache.
    /// </summary>
    public bool IsHit { get; set; }

    /// <summary>
    /// Indicates whether the requested entry was missing from the cache.
    /// </summary>
    public bool IsMiss => !IsHit;

    /// <summary>
    /// Gets the cached value when the entry exists.
    /// </summary>
    public T? Value { get; }

    public string? Version { get; private init; }

    public CacheReadResult(bool isHit, T? value)
    {
        IsHit = isHit;
        Value = value;
    }

    /// <summary>
    /// Creates a successful cache read result.
    /// </summary>
    /// <param name="value">The value retrieved from the cache.</param>
    /// <returns>A cache hit result containing the retrieved value.</returns>
    public static CacheReadResult<T> Hit(T? value) => new CacheReadResult<T>(true, value);

    /// <summary>
    /// Represents a cache entry that could not be found.
    /// </summary>
    public static CacheReadResult<T> Miss => new(false, default);

    public static CacheReadResult<T> MissAt(string version) => new(false, default) { Version = version };

}
