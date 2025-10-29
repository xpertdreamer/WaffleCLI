namespace WaffleCLI.Configuration;

/// <summary>
/// Provides configuration data for the application
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Gets a configuration section
    /// </summary>
    /// <typeparam name="T">Section type</typeparam>
    /// <param name="sectionName">Section name</param>
    /// <returns>Configuration section</returns>
    T GetSection<T>(string sectionName) where T : new();
    
    /// <summary>
    /// Gets a configuration section
    /// </summary>
    /// <param name="sectionName">Section name</param>
    /// <param name="sectionType">Section type</param>
    /// <returns>Configuration section</returns>
    object GetSection(string sectionName, Type sectionType);
    
    /// <summary>
    /// Binds configuration to an existing instance
    /// </summary>
    /// <param name="instance">Instance to bind to</param>
    /// <param name="sectionName">Section name</param>
    void Bind(object instance, string sectionName);
}