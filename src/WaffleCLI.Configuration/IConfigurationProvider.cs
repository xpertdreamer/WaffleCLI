namespace WaffleCLI.Configuration;

/// <summary>
/// Provides configuration data for the application from various sources with support for typed sections and binding.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Gets a strongly-typed configuration section with the specified name.
    /// If the section does not exist, returns a new instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the configuration section. Must have a parameterless constructor.</typeparam>
    /// <param name="sectionName">The name of the configuration section to retrieve.</param>
    /// <returns>A typed instance of the configuration section.</returns>
    T GetSection<T>(string sectionName) where T : new();
    
    /// <summary>
    /// Gets a configuration section with the specified name and type.
    /// </summary>
    /// <param name="sectionName">The name of the configuration section to retrieve.</param>
    /// <param name="sectionType">The type of the configuration section to return.</param>
    /// <returns>An object instance of the specified type containing the configuration data.</returns>
    object GetSection(string sectionName, Type sectionType);
    
    /// <summary>
    /// Binds configuration values from the specified section to an existing object instance.
    /// </summary>
    /// <param name="instance">The target object instance to bind configuration values to.</param>
    /// <param name="sectionName">The name of the configuration section to bind from.</param>
    void Bind(object instance, string sectionName);
}

/// <summary>
/// Base class for configuration sections that provides lifecycle hooks for loading and saving operations.
/// </summary>
public abstract class ConfigurationSection
{
    /// <summary>
    /// Called when the configuration section has been loaded and values have been populated.
    /// Override this method to perform initialization or validation after loading.
    /// </summary>
    public virtual void OnLoaded() { }
    
    /// <summary>
    /// Called when the configuration section is about to be saved.
    /// Override this method to perform cleanup or final modifications before saving.
    /// </summary>
    public virtual void OnSaving() { }
}