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
    /// Gets the help text for this command group
    /// </summary>
    string GetHelpText();
}