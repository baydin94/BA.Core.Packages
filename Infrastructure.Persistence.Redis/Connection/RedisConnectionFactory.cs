using Infrastructure.Persistence.Redis.Configuration;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Net;

namespace Infrastructure.Persistence.Redis.Connection;

public class RedisConnectionFactory : IDisposable
{
    //Log imp. ILogger<RedisConnectionFactory>

    private readonly RedisSettings _redisSettings;
    private readonly ConnectionSettings _connectionSettings;
    private readonly RedisPolicySettings _redisPolicySettings;

    //Pool
    private readonly ConcurrentBag<ConnectionMultiplexer> _pool = new();
    private readonly int _poolSize = 3;
    private bool _disposed;

    public RedisConnectionFactory(RedisSettings redisSettings, ConnectionSettings connectionSettings, RedisPolicySettings redisPolicySettings)
    {
        _redisSettings = redisSettings;
        _connectionSettings = connectionSettings;
        _redisPolicySettings = redisPolicySettings;
        //Log _logger = logger;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
            _pool.Add(CreateConnection());

    }

    public ConnectionMultiplexer CreateConnection()
    {
        ConfigurationOptions options = new ConfigurationOptions
        {
            AbortOnConnectFail = _connectionSettings.AbortOnConnectFail,
            ConnectRetry = _connectionSettings.ConnectRetry,
            AsyncTimeout = _connectionSettings.AsyncTimeout,
            SyncTimeout = _connectionSettings.SyncTimeout,
            KeepAlive = _connectionSettings.KeepAlive,
        };
        options.EndPoints.Add(_redisSettings.Configuration);

        //Reconnect policy
        options.ReconnectRetryPolicy = new ExponentialRetry(_connectionSettings.ReconnectRetryBaseDelay, _connectionSettings.ReconnectRetryMaxDelay);

        return ConnectionMultiplexer.Connect(options);
    }    

    private ConnectionMultiplexer GetConnection()
    {
        if (_pool.TryTake(out var mux))
        {
            if (!mux.IsConnected)
                mux = CreateConnection();

            _pool.Add(mux);
            return mux;
        }
        return CreateConnection();
    }

    public IDatabase GetDatabase(int db = -1) => GetConnection().GetDatabase(db);
    public IServer GetServer()
    {
        ConnectionMultiplexer connection = GetConnection();
        EndPoint endpoint = connection.GetEndPoints().First();
        return connection.GetServer(endpoint);
    }

    private void ConnectionFailedHandler(object? sender, ConnectionFailedEventArgs @event)
    {
        //log ör:
        //_logger.LogError("Redis connection failed. Endpoint: {Endpoint}, FailureType: {FailureType}, Ex: {Ex}",
        //    e.EndPoint, e.FailureType, e.Exception?.Message);
    }

    private void ConnectionRestoreHandler(object? sender, ConnectionFailedEventArgs @event)
    {
        //_logger.LogInformation("Redis connection restored. Endpoint: {Endpoint}", @event.EndPoint);
    }

    private void ErrorMessageHandler(object? sender, RedisErrorEventArgs @event)
    {
        //_logger.LogError("Redis error: {Message}", @event.Message);
    }

    private void ConfigurationChangedHandler(object? sender, EndPointEventArgs @event)
    {
        //_logger.LogWarning("Redis configuration changed: {Endpoint}", @event.EndPoint);
    }

    public void Dispose()
    {
        foreach (var mux in _pool)
            mux.Dispose();
    }
}
