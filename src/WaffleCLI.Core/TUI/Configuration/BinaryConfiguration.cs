using System.Text.Json.Serialization;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// Configuration for an external binary
    /// </summary>
    public class BinaryConfiguration
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("executablePath")]
        public string ExecutablePath { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = string.Empty;

        [JsonPropertyName("workingDirectory")]
        public string WorkingDirectory { get; set; } = string.Empty;

        [JsonPropertyName("environmentVariables")]
        public Dictionary<string, string>? EnvironmentVariables { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "General";

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "🚀";

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("requiresInput")]
        public bool RequiresInput { get; set; } = false;

        [JsonPropertyName("inputPrompt")]
        public string InputPrompt { get; set; } = "Enter input:";

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Validates the binary configuration
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(ExecutablePath))
                errors.Add("ExecutablePath is required");

            // Check if file exists (warning, not error)
            if (!File.Exists(ExecutablePath) && !IsInSystemPath(ExecutablePath))
                errors.Add($"Executable not found: {ExecutablePath}");

            return errors.Count == 0;
        }

        private bool IsInSystemPath(string executable)
        {
            if (File.Exists(executable))
                return true;

            // Check if executable is in system PATH
            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
            
            foreach (var dir in pathDirs)
            {
                if (string.IsNullOrWhiteSpace(dir))
                    continue;

                try
                {
                    var fullPath = Path.Combine(dir, executable);
                    if (File.Exists(fullPath))
                        return true;
                }
                catch
                {
                    // Invalid path, continue
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the command line for this binary
        /// </summary>
        public string GetCommandLine()
        {
            if (string.IsNullOrWhiteSpace(Arguments))
                return ExecutablePath;
            
            return $"{ExecutablePath} {Arguments}";
        }
    }

    /// <summary>
    /// Collection of binary configurations
    /// </summary>
    public class BinariesConfiguration
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [JsonPropertyName("binaries")]
        public List<BinaryConfiguration>? Binaries { get; set; }

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Gets all enabled binaries
        /// </summary>
        public List<BinaryConfiguration> GetEnabledBinaries()
        {
            return Binaries?.Where(b => b.Enabled).ToList() ?? new List<BinaryConfiguration>();
        }

        /// <summary>
        /// Gets binaries by category
        /// </summary>
        public Dictionary<string, List<BinaryConfiguration>> GetBinariesByCategory()
        {
            var enabled = GetEnabledBinaries();
            if (enabled.Count == 0)
                return new Dictionary<string, List<BinaryConfiguration>>();
                
            return enabled
                .GroupBy(b => b.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}