using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

namespace WaffleCLI.Core.TUI.Configuration;

public class ConfigurationManager : IDisposable
{
    private readonly ILogger<ConfigurationManager> _logger;
    private readonly string _configPath;
    private readonly FileSystemWatcher _configWatcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _configLock = new();
    private bool _disposed = false;
    
    public AppConfig Config { get; private set; } = new();
    public event Action<AppConfig>? ConfigChanged;

    public ConfigurationManager(ILogger<ConfigurationManager> logger, string configPath = "appsettings.json")
    {
        _logger = logger;
        _configPath = Path.GetFullPath(configPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        var directory = Path.GetDirectoryName(_configPath) ?? Directory.GetCurrentDirectory();
        var fileName = Path.GetFileName(_configPath);

        _configWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = Config?.Behavior?.ReloadOnConfigChange ?? true
        };

        _configWatcher.Changed += OnConfigFileChanged;
        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        lock (_configLock)
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    // Retry logic for file locks
                    for (var i = 0; i < 3; i++)
                    {
                        try
                        {
                            var json = File.ReadAllText(_configPath);
                            var newConfig = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions);
                            if (newConfig != null)
                            {
                                Config = newConfig;
                                _logger.LogInformation("Configuration loaded from {ConfigPath}", _configPath);
                                break;
                            }
                        }
                        catch (IOException) when (i < 2)
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
                else
                {
                    Config = new AppConfig();
                    SaveConfiguration();
                    _logger.LogInformation("Created default configuration at {ConfigPath}", _configPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration at {ConfigPath}, using defaults", _configPath);
                Config = new AppConfig();
            }
        }
    }

    public void SaveConfiguration()
    {
        if (_disposed) return;

        lock (_configLock)
        {
            try
            {
                _configWatcher.EnableRaisingEvents = false;

                var json = JsonSerializer.Serialize(Config, _jsonOptions);
                File.WriteAllText(_configPath, json);
                _logger.LogInformation("Configuration saved to {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration at {ConfigPath}", _configPath);
            }
            finally
            {
                _configWatcher.EnableRaisingEvents = Config.Behavior.ReloadOnConfigChange;
            }
        }
    }

    public T GetSection<T>(string sectionPath) where T : new()
    {
        try
        {
            var json = JsonSerializer.Serialize(Config, _jsonOptions);
            using var document = JsonDocument.Parse(json);
            
            var element = document.RootElement;
            foreach (var segment in sectionPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(segment, out var property))
                    element = property;
                else
                    return new T();
            }

            return element.Deserialize<T>(_jsonOptions) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    public void UpdateSection<T>(string sectionPath, T section)
    {
        // Implementation for updating specific sections would go here
        SaveConfiguration();
    }

    private async void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed || _disposed) return;

        // Debounce file change events
        await Task.Delay(100);
        
        try
        {
            _configWatcher.EnableRaisingEvents = false;
            LoadConfiguration();
            ConfigChanged?.Invoke(Config);
            _logger.LogInformation("Configuration reloaded due to file change");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration");
        }
        finally
        {
            _configWatcher.EnableRaisingEvents = Config.Behavior.ReloadOnConfigChange;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _configWatcher?.Dispose();
        GC.SuppressFinalize(this);
    }
}