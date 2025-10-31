using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Basic.Commands;

[Command("example", "An example command for your WaffleCLI application")]
public class ExampleCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public ExampleCommand(IConsoleOutput output)
    {
        _output = output;
    }

    public string Name => "example";
    public string Description => "An example command for your WaffleCLI application";

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _output.WriteSuccess("🎉 Hello from your WaffleCLI application!");
        _output.WriteLine($"You passed {args.Length} arguments: {string.Join(", ", args)}");
        
        if (args.Length > 0)
        {
            _output.WriteInfo($"First argument: {args[0]}");
        }
        
        return Task.CompletedTask;
    }
}