using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[Command("calc", "Perform basic calculations")]
public class CalcCommand : ICommand
{
    public string Name => "calc";
    public string Description => "Perform basic calculations";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: calc <number1> <operator> <number2>");
            Console.WriteLine("Supported operators: +, -, *, /");
            return Task.CompletedTask;
        }

        if (!double.TryParse(args[0], out var a) || !double.TryParse(args[2], out var b))
        {
            Console.WriteLine("Error: invalid number format");
            return Task.CompletedTask;
        }

        var result = args[1] switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" when b != 0 => a / b,
            "/" => double.NaN,
            _ => double.NaN
        };

        if (double.IsNaN(result))
        {
            Console.WriteLine("Error: invalid operation or division by zero");
            return Task.CompletedTask;
        }

        Console.WriteLine($"Result: {a} {args[1]} {b} = {result}");
        return Task.CompletedTask;
    }
}