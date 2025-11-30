using System.Text.Json;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// Loads configuration from JSON files
    /// </summary>
    public class JsonConfigurationLoader
    {
        public static TuiConfiguration LoadConfiguration(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<TuiConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new TuiConfiguration();
            }
            catch (Exception ex)
            {
                throw new TuiException($"Failed to load configuration from {filePath}", ex);
            }
        }

        public static void SaveConfiguration(TuiConfiguration config, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new TuiException($"Failed to save configuration to {filePath}", ex);
            }
        }
    }
}