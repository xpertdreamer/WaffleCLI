using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for performing basic arithmetic calculations
/// Supports addition, subtraction, multiplication, and division
/// </summary>
[Command("calc", "Perform calculations")]
public class CalcCommand : ICommand
{
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the CalcCommand class
    /// </summary>
    /// <param name="output">Console output service</param>
    public CalcCommand(IConsoleOutput output)
    {
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "calc";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Perform calculations";

    /// <summary>
    /// Executes the calculation command with provided arguments
    /// </summary>
    /// <param name="args">Arguments in format: number1 operator number2</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length != 3)
        {
            _output.WriteError("Usage: calc <number1> <operator> <number2>");
            _output.WriteInfo("Supported operators: +, -, *, /");
            return Task.CompletedTask;
        }

        if (!double.TryParse(args[0], out var a) || !double.TryParse(args[2], out var b))
        {
            _output.WriteError("Error: invalid number format");
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
            _output.WriteError("Error: invalid operation or division by zero");
            return Task.CompletedTask;
        }

        _output.WriteSuccess($"Result: {a} {args[1]} {b} = {result}");
        return Task.CompletedTask;
    }
}