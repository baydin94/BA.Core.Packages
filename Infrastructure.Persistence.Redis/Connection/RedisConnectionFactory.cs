using Infrastructure.Persistence.Redis.Configuration;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Persistence.Redis.Connection;

public class RedisConnectionFactory : IDisposable
{
    private readonly ConfigurationOptions _options;
    private readonly object _sync = new();
    private ConnectionMultiplexer? _connection;
    private bool _disposed;

    public RedisConnectionFactory(RedisSettings settings, ConnectionSettings connection, RedisPolicySettings policy)
    {
        _options = BuildOptions(settings, connection, policy);
    }

    internal static ConfigurationOptions BuildOptions(
        RedisSettings settings, ConnectionSettings connection, RedisPolicySettings policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.InstanceName);
        ArgumentOutOfRangeException.ThrowIfNegative(settings.Database);
        ArgumentOutOfRangeException.ThrowIfNegative(connection.ConnectRetry);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connection.ConnectTimeout);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connection.AsyncTimeout);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connection.SyncTimeout);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connection.ReconnectRetryBaseDelay);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            connection.ReconnectRetryMaxDelay, connection.ReconnectRetryBaseDelay);

        var options = new ConfigurationOptions
        {
            User = settings.User,
            Password = settings.Password,
            Ssl = settings.Ssl,
            DefaultDatabase = settings.Database,
            AbortOnConnectFail = connection.AbortOnConnectFail,
            ConnectRetry = connection.ConnectRetry,
            ConnectTimeout = connection.ConnectTimeout,
            AsyncTimeout = connection.AsyncTimeout,
            SyncTimeout = connection.SyncTimeout,
            KeepAlive = connection.KeepAlive,
            ReconnectRetryPolicy = new ExponentialRetry(
                connection.ReconnectRetryBaseDelay, connection.ReconnectRetryMaxDelay),
            AllowAdmin = policy.AllowAdmin,
            ResolveDns = policy.ResolveDns
        };
        if (settings.Endpoints is { Length: > 0 } endpoints)
        {
            for (int i = 0; i < endpoints.Length; i++)
                options.EndPoints.Add(ParseEndpoint(endpoints[i], i));
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.Host);
            ArgumentOutOfRangeException.ThrowIfLessThan(settings.Port, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(settings.Port, 65535);
            options.EndPoints.Add(settings.Host, settings.Port);
        }
        return options;
    }

    private static EndPoint ParseEndpoint(string? input, int index)
    {
        var address = input?.Trim() ?? string.Empty;
        int separator = address.LastIndexOf(':');
        if (separator > 0 &&
            int.TryParse(address.AsSpan(separator + 1), NumberStyles.None,
                CultureInfo.InvariantCulture, out int port) && port is >= 1 and <= 65535)
        {
            var host = address[..separator];
            if (host.StartsWith('[') && host.EndsWith(']') &&
                IPAddress.TryParse(host[1..^1], out var ip) &&
                ip.AddressFamily == AddressFamily.InterNetworkV6)
                return new IPEndPoint(ip, port);

            var hostType = Uri.CheckHostName(host);
            if (hostType == UriHostNameType.IPv4)
                return new IPEndPoint(IPAddress.Parse(host), port);
            if (hostType == UriHostNameType.Dns)
                return new DnsEndPoint(host, port);
        }

        throw new ArgumentException($"Redis:Endpoints:{index} must contain a host:port or [IPv6]:port address.");
    }

    /// <summary>Creates an independent connection owned by the caller.</summary>
    public ConnectionMultiplexer CreateConnection() => ConnectionMultiplexer.Connect(_options);

    private ConnectionMultiplexer GetConnection()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _connection ??= CreateConnection();
        }
    }

    public IDatabase GetDatabase(int db = -1) => GetConnection().GetDatabase(db);

    public IServer GetServer()
    {
        var connection = GetConnection();
        return connection.GetServer(connection.GetEndPoints().First());
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            _connection?.Dispose();
        }
    }
}
