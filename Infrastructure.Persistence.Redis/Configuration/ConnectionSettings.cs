namespace Infrastructure.Persistence.Redis.Configuration;

public class ConnectionSettings
{
    public bool AbortOnConnectFail { get; set; } = false;
    public int ConnectRetry { get; set; } = 5;
    public int ConnectTimeout { get; set; } = 5000;
    public int KeepAlive { get; set; } = 180;
    public int SyncTimeout { get; set; } = 5000;
    public int AsyncTimeout { get; set; } = 5000;
    public int ReconnectRetryPolicy { get; set; } = 5000;
    public int ReconnectRetryBaseDelay { get; set; } = 500;   // ilk retry
    public int ReconnectRetryMaxDelay { get; set; } = 5000;   // max retry
}
