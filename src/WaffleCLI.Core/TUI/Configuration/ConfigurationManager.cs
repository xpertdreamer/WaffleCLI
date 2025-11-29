using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WaffleCLI.Core.TUI.Configuration;

public class ConfigurationManager
{
    private readonly ILogger<ConfigurationManager> _logger;
    private readonly string _configPath;
    private readonly FileSystemWatcher _configWatcher;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AppConfig Config { get; private set; }
    public event Action<AppConfig>? ConfigChanged;

    public ConfigurationManager(ILogger<ConfigurationManager> logger, string configPath = "appsettings.json")
    {
        _logger = logger;
        _configPath = configPath;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        _configWatcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(Path.GetFullPath(_configPath)) ?? ".",
            Filter = Path.GetFileName(_configPath),
            NotifyFilter = NotifyFilters.LastWrite
        };

        _configWatcher.Changed += OnConfigFileChanged;
        _configWatcher.EnableRaisingEvents = true;

        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                Config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
                _logger.LogInformation("Configuration loaded from {ConfigPath}", _configPath);
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

    public void SaveConfiguration()
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
            _configWatcher.EnableRaisingEvents = true;
        }
    }

    public T GetSection<T>(string sectionPath) where T : new()
    {
        try
        {
            var json = JsonSerializer.Serialize(Config, _jsonOptions);
            var document = JsonDocument.Parse(json);
            var path = sectionPath.Replace(':', '.');
            var element = document.RootElement;

            foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
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
        SaveConfiguration();
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            Thread.Sleep(100);
            try
            {
                LoadConfiguration();
                ConfigChanged?.Invoke(Config);
                _logger.LogInformation("Configuration reloaded due to file change");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload configuration");
            }
        }
    }

    public void Dispose()
    {
        _configWatcher?.Dispose();
    }
}