namespace WaffleCLI.SampleApp;

public class AppSettings
{
    public string AppName { get; init; } = "Console Framework Sample";
    public string Version { get; init; } = "1.0.0";
    public string DefaultUser { get; init; } = "Admin";
    public int MaxConnections { get; init; } = 10;
    public FeatureSettings EnableFeatures { get; init; } = new();
}

public class FeatureSettings
{
    public bool Logging { get; set; } = true;
    public bool Metrics { get; set; } = false;
    public bool AutoUpdate { get; set; } = true;
}
//
// public class DatabaseSettings
// {
//     public string ConnectionString { get; set; } = "Server=localhost;Database=SampleApp;Trusted_Connection=true;";
//     public int Timeout { get; set; } = 30;
//     public bool EnableLogging { get; set; } = true;
// }
//
// public class WeatherSettings
// {
//     public string ApiKey { get; set; } = "your-api-key-here";
//     public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1";
//     public string DefaultCity { get; set; } = "Moscow";
// }