namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Provides command registration and resolution functionality
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Registers a command type with the registry
    /// </summary>
    /// <typeparam name="TCommand">The type of command to register</typeparam>
    void RegisterCommand<TCommand>() where TCommand : ICommand;

    /// <summary>
    /// Registers a command type with the registry
    /// </summary>
    /// <param name="commandType">The type of command to register</param>
    void RegisterCommand(Type commandType);

    /// <summary>
    /// Retrieves a command instance by name
    /// </summary>
    /// <param name="name">The name of the command</param>
    /// <returns>The command instance or null if not found</returns>
    ICommand? GetCommand(string name);

    /// <summary>
    /// Retrieves all registered command instances
    /// </summary>
    /// <returns>Read-only collection of commands</returns>
    IReadOnlyCollection<ICommand> GetCommands();
}