using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[Command("waffle", "Make a waffle")]
public class WaffleCommand : ICommand
{
    public string Name => "waffle";
    public string Description => "Make a waffle";

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🧇 Preparing waffle batter...");
        await Task.Delay(1000, cancellationToken);
        
        Console.WriteLine("🔥 Heating waffle iron...");
        await Task.Delay(800, cancellationToken);
        
        Console.WriteLine("🍯 Adding batter to iron...");
        await Task.Delay(1200, cancellationToken);
        
        Console.WriteLine("🔄 Flipping waffle...");
        await Task.Delay(600, cancellationToken);
        
        Console.WriteLine("🎉 Waffle is ready! Golden and crispy!");
        await Task.CompletedTask;
    }
}