using Infrastructure.Persistence.Redis.Configuration;
using Infrastructure.Persistence.Redis.Connection;
using Infrastructure.Persistence.Redis.Constants;
using Infrastructure.Persistence.Redis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis;

public static class RedisPersistenceServiceRegistration
{
    public static IServiceCollection AddRedisPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Redis").Get<RedisSettings>() ?? throw new InvalidOperationException(RedisExceptionMessages.RedisConfigurationMissing);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = settings.Configuration;
            options.InstanceName = settings.InstanceName;
        });

        services.AddSingleton<RedisConnectionFactory>();
        services.AddSingleton<IRedisService, RedisService>();

        return services;
    }
}
