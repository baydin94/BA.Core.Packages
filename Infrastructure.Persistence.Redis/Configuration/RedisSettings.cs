namespace Infrastructure.Persistence.Redis.Configuration;

public class RedisSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Password { get; set; }
    public string Configuration => $"{Host}:{Port}";
    public string InstanceName { get; set; } = "App_";

}
