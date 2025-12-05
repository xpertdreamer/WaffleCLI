using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// Manages configuration of external binaries
    /// </summary>
    public class BinariesManager
    {
        private readonly string _configPath;
        private BinariesConfiguration? _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public BinariesConfiguration Configuration => _configuration;
        public string ConfigPath => _configPath;

        public event EventHandler<BinariesChangedEventArgs> BinariesChanged;

        public BinariesManager(string configPath = null)
        {
            _configPath = configPath ?? "binaries.json";
            
            // Proper JSON options for .NET 8 with reflection enabled
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // Enable reflection-based serialization for .NET 8
                TypeInfoResolver = CreateTypeInfoResolver()
            };

            LoadConfiguration();
        }

        private static IJsonTypeInfoResolver CreateTypeInfoResolver()
        {
            // For .NET 8, we need to explicitly create a resolver
            return new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddSerializationModifiers }
            };
        }

        private static void AddSerializationModifiers(JsonTypeInfo typeInfo)
        {
            // Add any custom modifiers if needed
        }

        /// <summary>
        /// Loads configuration from file
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    
                    // Use JsonSerializer with our options
                    _configuration = JsonSerializer.Deserialize<BinariesConfiguration>(json, _jsonOptions);
                    
                    if (_configuration == null)
                    {
                        _configuration = CreateDefaultConfiguration();
                        TuiLogger.LogInfo("Configuration was null, created default");
                    }
                    else
                    {
                        // Ensure collections are initialized
                        _configuration.Binaries ??= new List<BinaryConfiguration>();
                        _configuration.Categories ??= new List<string>();
                        
                        TuiLogger.LogInfo($"Loaded {_configuration.Binaries.Count} binaries from {_configPath}");
                    }
                }
                else
                {
                    _configuration = CreateDefaultConfiguration();
                    SaveConfiguration();
                    TuiLogger.LogInfo($"Created default binaries configuration at {_configPath}");
                }
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to load binaries configuration from {_configPath}", ex);
                _configuration = CreateDefaultConfiguration();
            }
        }

        /// <summary>
        /// Saves configuration to file
        /// </summary>
        public bool SaveConfiguration()
        {
            try
            {
                _configuration.LastUpdated = DateTime.Now;
                
                // Ensure collections exist
                _configuration.Binaries ??= new List<BinaryConfiguration>();
                _configuration.Categories ??= new List<string>();
                
                var json = JsonSerializer.Serialize(_configuration, _jsonOptions);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(_configPath, json);
                TuiLogger.LogInfo($"Saved {_configuration.Binaries.Count} binaries to {_configPath}");
                
                BinariesChanged?.Invoke(this, new BinariesChangedEventArgs());
                return true;
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to save binaries configuration to {_configPath}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adds or updates a binary configuration
        /// </summary>
        public bool AddOrUpdateBinary(BinaryConfiguration binary)
        {
            if (binary == null)
                throw new ArgumentNullException(nameof(binary));

            // Ensure Binaries list exists
            _configuration.Binaries ??= new List<BinaryConfiguration>();
            
            var existingIndex = _configuration.Binaries.FindIndex(b => b.Id == binary.Id);
            
            if (existingIndex >= 0)
            {
                _configuration.Binaries[existingIndex] = binary;
                TuiLogger.LogInfo($"Updated binary: {binary.Name}");
            }
            else
            {
                if (string.IsNullOrEmpty(binary.Id))
                    binary.Id = Guid.NewGuid().ToString();
                    
                _configuration.Binaries.Add(binary);
                TuiLogger.LogInfo($"Added binary: {binary.Name}");
            }

            // Update categories list
            UpdateCategories();
            
            return SaveConfiguration();
        }

        /// <summary>
        /// Removes a binary by ID
        /// </summary>
        public bool RemoveBinary(string binaryId)
        {
            if (_configuration.Binaries == null)
                return false;
                
            var removedCount = _configuration.Binaries.RemoveAll(b => b.Id == binaryId);
            
            if (removedCount > 0)
            {
                TuiLogger.LogInfo($"Removed binary with ID: {binaryId}");
                UpdateCategories();
                return SaveConfiguration();
            }
            
            return false;
        }

        /// <summary>
        /// Gets a binary by ID
        /// </summary>
        public BinaryConfiguration GetBinary(string binaryId)
        {
            return _configuration.Binaries?.FirstOrDefault(b => b.Id == binaryId);
        }
        
        /// <summary>
        /// Gets enabled binaries with null safety
        /// </summary>
        public List<BinaryConfiguration> GetEnabledBinaries()
        {
            return _configuration?.Binaries?
                .Where(b => b?.Enabled == true)
                .ToList() ?? new List<BinaryConfiguration>();
        }

        /// <summary>
        /// Gets binaries grouped by category with null safety
        /// </summary>
        public Dictionary<string, List<BinaryConfiguration>> GetBinariesByCategory()
        {
            var enabled = GetEnabledBinaries();
            if (enabled.Count == 0)
                return new Dictionary<string, List<BinaryConfiguration>>();
        
            return enabled
                .Where(b => b != null && !string.IsNullOrEmpty(b.Category))
                .GroupBy(b => b!.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Searches binaries by term with null safety
        /// </summary>
        public List<BinaryConfiguration> SearchBinaries(string searchTerm)
        {
            var enabled = GetEnabledBinaries();
            if (enabled.Count == 0)
                return enabled;

            if (string.IsNullOrWhiteSpace(searchTerm))
                return enabled;

            var term = searchTerm.ToLowerInvariant();
            return enabled
                .Where(b => b != null &&
                            ((b.Name?.ToLowerInvariant().Contains(term) ?? false) ||
                             (b.Description?.ToLowerInvariant().Contains(term) ?? false) ||
                             (b.ExecutablePath?.ToLowerInvariant().Contains(term) ?? false) ||
                             (b.Tags?.Any(t => t?.ToLowerInvariant().Contains(term) == true) ?? false)))
                .ToList();
        }

        /// <summary>
        /// Validates all binaries in the configuration
        /// </summary>
        public List<BinaryValidationResult> ValidateAllBinaries()
        {
            var results = new List<BinaryValidationResult>();
            
            if (_configuration.Binaries == null)
                return results;
            
            foreach (var binary in _configuration.Binaries)
            {
                var result = new BinaryValidationResult(binary);
                binary.Validate(out var errors);
                result.Errors.AddRange(errors);
                results.Add(result);
            }
            
            return results;
        }

        /// <summary>
        /// Imports binaries from a directory
        /// </summary>
        public int ImportFromDirectory(string directoryPath, string category = "Imported")
        {
            if (!Directory.Exists(directoryPath))
            {
                TuiLogger.LogError($"Directory not found: {directoryPath}");
                return 0;
            }

            int importedCount = 0;
            
            try
            {
                var executables = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                    .Where(IsExecutableFile)
                    .ToList();

                foreach (var executable in executables)
                {
                    var binaryName = Path.GetFileNameWithoutExtension(executable);
                    
                    // Check if already exists
                    if (_configuration.Binaries != null && 
                        _configuration.Binaries.Any(b => 
                            b.ExecutablePath?.Equals(executable, StringComparison.OrdinalIgnoreCase) ?? false))
                    {
                        continue;
                    }

                    var binary = new BinaryConfiguration
                    {
                        Name = binaryName,
                        Description = $"Imported from {directoryPath}",
                        ExecutablePath = executable,
                        Category = category,
                        Icon = "📁"
                    };

                    AddOrUpdateBinary(binary);
                    importedCount++;
                }

                TuiLogger.LogInfo($"Imported {importedCount} binaries from {directoryPath}");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to import binaries from {directoryPath}", ex);
            }

            return importedCount;
        }

        private static bool IsExecutableFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
    
            // Platform-agnostic check for common executable extensions
            var executableExtensions = new HashSet<string> 
            { 
                ".exe", ".bat", ".cmd", ".com", ".sh", ".bash", 
                ".run", ".bin", ".out", ".app", ".py", ".pl", 
                ".rb", ".js", ".vbs", ".ps1"
            };
    
            if (executableExtensions.Contains(extension))
            {
                return true;
            }
    
            // Check if file exists
            if (!File.Exists(filePath))
            {
                return false;
            }
    
            // Platform-specific checks
            if (OperatingSystem.IsWindows())
            {
                // Windows: check file attributes or extension
                return extension == ".exe" || extension == ".bat" || 
                       extension == ".cmd" || extension == ".com";
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                try
                {
                    // Check if file has execute permissions
                    var fileInfo = new FileInfo(filePath);
                    var mode = fileInfo.UnixFileMode;
            
                    return (mode & UnixFileMode.UserExecute) != 0 ||
                           (mode & UnixFileMode.GroupExecute) != 0 ||
                           (mode & UnixFileMode.OtherExecute) != 0;
                }
                catch (PlatformNotSupportedException)
                {
                    // Fallback for systems without UnixFileMode support
                    // Check for common executable patterns
                    return string.IsNullOrEmpty(extension) || 
                           extension == ".sh" || extension == ".bin" || 
                           extension == ".run" || extension == ".out" ||
                           !extension.Contains('.');
                }
                catch (Exception ex)
                {
                    Infrastructure.Logging.TuiLogger.LogError($"Failed to check file permissions for {filePath}", ex);
                    return false;
                }
            }
    
            // Unknown platform - use extension check only
            return false;
        }

        private void UpdateCategories()
        {
            if (_configuration == null)
                return;
                
            if (_configuration.Binaries == null)
            {
                _configuration.Categories = new List<string>();
                return;
            }
            
            _configuration.Categories = _configuration.Binaries
                .Select(b => b.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        private BinariesConfiguration CreateDefaultConfiguration()
        {
            var config = new BinariesConfiguration();
            
            // Initialize collections
            config.Binaries = new List<BinaryConfiguration>();
            config.Categories = new List<string>();
            
            // Add some example binaries based on platform
            if (OperatingSystem.IsWindows())
            {
                config.Binaries.AddRange(GetWindowsDefaultBinaries());
            }
            else if (OperatingSystem.IsLinux())
            {
                config.Binaries.AddRange(GetLinuxDefaultBinaries());
            }
            else if (OperatingSystem.IsMacOS())
            {
                config.Binaries.AddRange(GetMacOSDefaultBinaries());
            }

            UpdateCategories();
            return config;
        }

        private List<BinaryConfiguration> GetWindowsDefaultBinaries()
        {
            return new List<BinaryConfiguration>
            {
                new BinaryConfiguration
                {
                    Id = "cmd",
                    Name = "Command Prompt",
                    Description = "Windows Command Prompt",
                    ExecutablePath = "cmd.exe",
                    Arguments = "/k",
                    Category = "System",
                    Icon = "💻",
                    Enabled = true
                },
                new BinaryConfiguration
                {
                    Id = "powershell",
                    Name = "PowerShell",
                    Description = "Windows PowerShell",
                    ExecutablePath = "powershell.exe",
                    Arguments = "-NoExit",
                    Category = "System",
                    Icon = "🔷",
                    Enabled = true
                }
            };
        }

        private List<BinaryConfiguration> GetLinuxDefaultBinaries()
        {
            return new List<BinaryConfiguration>
            {
                new BinaryConfiguration
                {
                    Id = "bash",
                    Name = "Bash Shell",
                    Description = "Linux Bash Shell",
                    ExecutablePath = "/bin/bash",
                    Arguments = "--login",
                    Category = "System",
                    Icon = "💻",
                    Enabled = true
                },
                new BinaryConfiguration
                {
                    Id = "ls",
                    Name = "List Files",
                    Description = "List directory contents",
                    ExecutablePath = "/bin/ls",
                    Arguments = "-la",
                    Category = "System",
                    Icon = "📁",
                    Enabled = true
                }
            };
        }

        private List<BinaryConfiguration> GetMacOSDefaultBinaries()
        {
            return new List<BinaryConfiguration>
            {
                new BinaryConfiguration
                {
                    Id = "zsh",
                    Name = "Zsh Shell",
                    Description = "macOS Zsh Shell",
                    ExecutablePath = "/bin/zsh",
                    Arguments = "--login",
                    Category = "System",
                    Icon = "💻",
                    Enabled = true
                },
                new BinaryConfiguration
                {
                    Id = "ls_mac",
                    Name = "List Files",
                    Description = "List directory contents",
                    ExecutablePath = "/bin/ls",
                    Arguments = "-la",
                    Category = "System",
                    Icon = "📁",
                    Enabled = true
                }
            };
        }
    }

    /// <summary>
    /// Event arguments for binaries changed event
    /// </summary>
    public class BinariesChangedEventArgs : EventArgs
    {
        // Can be extended with details about what changed
    }

    /// <summary>
    /// Result of binary validation
    /// </summary>
    public class BinaryValidationResult
    {
        public BinaryConfiguration Binary { get; }
        public List<string> Errors { get; } = new();
        public bool IsValid => Errors.Count == 0;

        public BinaryValidationResult(BinaryConfiguration binary)
        {
            Binary = binary;
        }
    }
}