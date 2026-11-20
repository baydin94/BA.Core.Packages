using Core.Abstractions.Applications.Caching;
using Infrastructure.Persistence.Redis.Adapters;
using Infrastructure.Persistence.Redis.Configuration;
using Infrastructure.Persistence.Redis.Connection;
using Infrastructure.Persistence.Redis.Constants;
using Infrastructure.Persistence.Redis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Persistence.Redis;

public static class RedisPersistenceServiceRegistration
{
    public static IServiceCollection AddRedisPersistenceServices(
        this IServiceCollection services, IConfiguration configuration)
        => services.AddRedisPersistenceServices(configuration.GetSection("Redis"));

    public static IServiceCollection AddRedisPersistenceServices(
        this IServiceCollection services, IConfigurationSection section)
    {
        var settings = section.Get<RedisSettings>()
            ?? throw new InvalidOperationException(RedisExceptionMessages.RedisConfigurationMissing);
        var options = RedisConnectionFactory.BuildOptions(settings, settings.Connection, settings.Policy);

        services.AddStackExchangeRedisCache(cache =>
        {
            cache.ConfigurationOptions = options;
            cache.InstanceName = settings.InstanceName;
        });

        services.TryAddSingleton(settings);
        services.TryAddSingleton(settings.Connection);
        services.TryAddSingleton(settings.Policy);
        services.TryAddSingleton<RedisConnectionFactory>();
        services.TryAddSingleton<IRedisService, RedisService>();
        services.TryAddSingleton<ICacheService, RedisCacheAdapter>();
        return services;
    }
}
