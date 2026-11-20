namespace Core.Abstractions.Applications.Caching;

/// <summary>The cache provider could not complete an operation due to a connectivity failure.</summary>
public sealed class CacheUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
