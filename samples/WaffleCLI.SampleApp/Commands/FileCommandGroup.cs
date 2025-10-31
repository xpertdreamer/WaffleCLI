using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Services;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command group for file operations
/// </summary>
public class FileCommandGroup : CommandGroup
{
    public FileCommandGroup(IServiceProvider serviceProvider, IConsoleOutput output) 
        : base(output, serviceProvider)
    {
        RegisterSubCommand<FileListCommand>("list");
        RegisterSubCommand<FileInfoCommand>("info");
        RegisterSubCommand<FileCopyCommand>("copy");
        RegisterSubCommand<FileDeleteCommand>("delete");
    }
    
    public override string Name => "file";
    public override string Description => "File management operations";
}