namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Represents a group of related commands
/// </summary>
public interface ICommandGroup : ICommand
{
    /// <summary>
    /// Gets the subcommands in this group
    /// </summary>
    IReadOnlyDictionary<string, ICommand> SubCommands { get; }
    
    /// <summary>
    /// Registers a subcommand in this group
    /// </summary>
    void RegisterSubCommand(string name, ICommand command);
    
    /// <summary>
    /// Gets the help text for this command group
    /// </summary>
    string GetHelpText();
}