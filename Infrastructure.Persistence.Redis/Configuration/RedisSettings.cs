namespace Infrastructure.Persistence.Redis.Configuration;

public class RedisSettings
{
    /// <summary>Cluster seed addresses in host:port or [IPv6]:port form.</summary>
    public string[] Endpoints { get; set; } = [];
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? User { get; set; }
    public string? Password { get; set; }
    public bool Ssl { get; set; }
    public int Database { get; set; }
    public string Configuration => Endpoints is { Length: > 0 }
        ? string.Join(",", Endpoints)
        : $"{Host}:{Port}";
    public string InstanceName { get; set; } = "App_";
    public ConnectionSettings Connection { get; set; } = new();
    public RedisPolicySettings Policy { get; set; } = new();
}
