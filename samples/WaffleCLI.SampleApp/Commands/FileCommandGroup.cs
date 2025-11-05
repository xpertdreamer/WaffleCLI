using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Commands;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[CommandGroup("file", "File operations")]
public class FileCommandGroup : CommandGroup
{
    public FileCommandGroup(IServiceProvider serviceProvider, IConsoleOutput output) 
        : base(serviceProvider, output) { }

    public override string Name => "file";
    public override string Description => "File management operations";
}