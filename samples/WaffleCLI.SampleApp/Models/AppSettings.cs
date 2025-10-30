namespace WaffleCLI.SampleApp.Models;

/// <summary>
/// Represents main application configuration settings
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the application name
    /// </summary>
    public string AppName { get; set; } = "WaffleCLI Sample App";
    
    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    public string Version { get; set; } = "1.0.0.0";
    
    /// <summary>
    /// Gets or sets the default user name
    /// </summary>
    public string DefaultUser { get; set; } = "Admin";
    
    /// <summary>
    /// Gets or sets the maximum number of connections
    /// </summary>
    public int MaxConnections { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the feature enablement settings
    /// </summary>
    public FeatureSettings EnableFeatures { get; set; } = new();
}

/// <summary>
/// Represents feature toggle configuration settings
/// </summary>
public class FeatureSettings
{
    /// <summary>
    /// Gets or sets whether logging is enabled
    /// </summary>
    public bool Logging { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether metrics collection is enabled
    /// </summary>
    public bool Metrics { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether auto-update is enabled
    /// </summary>
    public bool AutoUpdate { get; set; } = true;
}

/// <summary>
/// Represents database configuration settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the database connection string
    /// </summary>
    public string ConnectionString { get; set; } = "Server=localhost;Database=SampleApp;Trusted_Connection=true;";
    
    /// <summary>
    /// Gets or sets the command timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets whether database logging is enabled
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Represents weather API configuration settings
/// </summary>
public class WeatherSettings
{
    /// <summary>
    /// Gets or sets the weather API key
    /// </summary>
    public string ApiKey { get; set; } = "your-api-key-here";
    
    /// <summary>
    /// Gets or sets the weather API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1";
    
    /// <summary>
    /// Gets or sets the default city for weather queries
    /// </summary>
    public string DefaultCity { get; set; } = "Moscow";
}