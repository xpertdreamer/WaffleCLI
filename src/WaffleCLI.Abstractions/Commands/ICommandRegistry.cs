namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Manages command registration and retrieval
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Registers a command type
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    void RegisterCommand<TCommand>() where TCommand : ICommand;
    
    /// <summary>
    /// Gets a command by name
    /// </summary>
    /// <param name="name">Command name</param>
    /// <returns>Command instance or null</returns>
    ICommand? GetCommand(string name);
    
    /// <summary>
    /// Gets all registered commands
    /// </summary>
    /// <returns>Collection of commands</returns>
    IReadOnlyCollection<ICommand> GetCommands();
}