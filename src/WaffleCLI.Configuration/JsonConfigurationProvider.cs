using Microsoft.Extensions.Configuration;

namespace WaffleCLI.Configuration;

/// <summary>
/// JSON-based configuration provider
/// </summary>
public class JsonConfigurationProvider : IConfigurationProvider
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the JsonConfigurationProvider class
    /// </summary>
    /// <param name="configurationFileName">Configuration file path</param>
    public JsonConfigurationProvider(string configurationFileName = "appsettings.json")
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configurationFileName, optional:true, reloadOnChange:true)
            .AddEnvironmentVariables()
            .Build();
    }
    
    /// <inheritdoc />
    public T GetSection<T>(string sectionName) where T : new()
    {
        var section = new T();
        _configuration.GetSection(sectionName).Bind(section);
        return section;
    }

    /// <inheritdoc />
    public object GetSection(string sectionName, Type sectionType)
    {
        var section = Activator.CreateInstance(sectionType) ?? throw new InvalidOperationException($"Could not create instance of {sectionType}");
        
        _configuration.GetSection(sectionName).Bind(section);
        return section;
    }

    /// <inheritdoc />
    public void Bind(object instance, string sectionName)
    {
        _configuration.GetSection(sectionName).Bind(instance);
    }
}