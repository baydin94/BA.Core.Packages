namespace Infrastructure.Persistence.Redis.Configuration;

public class RedisPolicySettings
{
    public bool AllowAdmin { get; set; } = false;
    public bool ResolveDns { get; set; } = true;
}
