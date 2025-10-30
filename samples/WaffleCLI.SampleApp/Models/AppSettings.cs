namespace WaffleCLI.SampleApp.Models;

public class AppSettings
{
    public string AppName {get; set;} = "WaffleCLI Sample App";
    public string Version {get; set;} = "1.0.0.0";
    public string DefaultUser { get; set; } = "Admin";
    public int MaxConnections { get; set; } = 10;
    public FeatureSettings EnabledFeatures { get; set; } = new();
}

public class FeatureSettings
{
    public bool Logging { get; set; } = true;
    public bool Metrics {get; set;} = false;
    public bool AutoUpdate {get; set;} = true;
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=SampleApp;Trusted_Connection=true;";
    public int Timeout { get; set; } = 30;
    public bool EnableLogging { get; set; } = true;
}

public class WeatherSettings
{
    public string ApiKey { get; set; } = "your-api-key-here";
    public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1";
    public string DefaultCity { get; set; } = "Moscow";
}