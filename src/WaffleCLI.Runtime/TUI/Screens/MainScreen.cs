using System.Drawing;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI;
using WaffleCLI.Runtime.TUI.Elements;

namespace WaffleCLI.Runtime.TUI.Screens;

public class MainScreen : ITuiScreen
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger<MainScreen> _logger;

    private TuiListView _commandsListView;
    private TuiTextView _outputTextView;
    private TuiTextField _commandTextField;
    private List<TuiElement> _elements;
    private int _focusedElementIndex = 0;

    private List<ICommand> _commands = [];
    
    public string Title => "WaffleCLI TUI";

    public MainScreen(ICommandRegistry commandRegistry, ICommandExecutor commandExecutor, ILogger<MainScreen> logger)
    {
        _commandRegistry = commandRegistry;
        _commandExecutor = commandExecutor;
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _commands = _commandRegistry.GetCommands().ToList();

        _commandsListView = new TuiListView
        {
            X = 1,
            Y = 2,
            Width = 40,
            Height = Console.WindowHeight - 6,
            BackgroundColor = ConsoleColor.Black,
            ForegroundColor = ConsoleColor.White
        };
        
        _outputTextView = new TuiTextView
        {
            X = 42,
            Y = 2,
            Width = Console.WindowWidth - 43,
            Height = Console.WindowHeight - 6,
            BackgroundColor = ConsoleColor.Black,
            ForegroundColor = ConsoleColor.Gray
        };

        _commandTextField = new TuiTextField
        {
            X = 1,
            Y = Console.WindowHeight - 3,
            Width = Console.WindowWidth - 2,
            Height = 3,
            PlaceHolder = "Type command here or select from list...",
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.White
        };

        _commandsListView.ItemSelected += OnCommandSelected;
        _commandTextField.TextSubmitted += OnCommandSubmitted;
        
        _elements = [_commandsListView, _outputTextView, _commandTextField];
        UpdateFocus();
        
        _commandsListView.SetItems(_commands.Select(c =>
            $"{c.Name} - {c.Description}").ToList());
        
        _outputTextView.AppendLine("=== WaffleCLI TUI ===");
        _outputTextView.AppendLine($"Loaded {_commands.Count} _commands");
        _outputTextView.AppendLine("Use Tab == navigate, Up/Down Arrows == select, Enter == execute");
        
        return Task.CompletedTask;
    }

    public Task RenderAsync()
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.SetCursorPosition(0, 0);
        Console.Write(" WaffleCli TUI ".PadRight(Console.WindowWidth, '='));
        
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(" Tab:Navigate Up/Down Arrows:Select Enter:Execute Ctrl+Q:Quit ".PadRight(Console.WindowWidth, ' '));

        foreach (var element in _elements)
        {
            element.Render();
        }
        
        return Task.CompletedTask;
    }

    public Task HandleKeyAsync(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Q && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }

        if (keyInfo.Key == ConsoleKey.Tab)
        {
            _focusedElementIndex = (_focusedElementIndex + 1) % _elements.Count;
            UpdateFocus();
            return Task.CompletedTask;
        }

        var focusedElement = _elements[_focusedElementIndex];
        if (focusedElement.HandleKey(keyInfo))
        {
            return RenderAsync();
        }
        
        return Task.CompletedTask;
    }

    private void UpdateFocus()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            if (_elements[i] is TuiTextField textField)
            {
                textField.HasFocus = (i == _focusedElementIndex);
            }
        }
    }

    private void OnCommandSelected(int index)
    {
        if (index < 0 || index >= _commands.Count) return;
        var command = _commands[index];
        _commandTextField.Text = command.Name;
        _focusedElementIndex = _elements.IndexOf(_commandTextField);
        UpdateFocus();
    }

    private async void OnCommandSubmitted(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText)) return;
        
        _outputTextView.AppendLine($"$ {commandText}");

        try
        {
            var originalOut = Console.Out;
            await using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            var result = await _commandExecutor.ExecuteAsync(commandText);
            
            Console.SetOut(originalOut);
            
            var output = stringWriter.ToString();
            if (!string.IsNullOrWhiteSpace(output)) _outputTextView.AppendLine(output);
            
            if (!string.IsNullOrEmpty(result.Message)) _outputTextView.AppendLine(result.Message);
            
            _outputTextView.AppendLine(result.Success ? "Command completed successfully" : $"Command failed (exit code: {result.ExitCode})");
        }
        catch (Exception ex)
        {
            _outputTextView.AppendLine($"Error: {ex.Message}");
            _logger.LogError(ex, "Error executing command: {commandText}", commandText);
        }
        
        _commandTextField.Text = string.Empty;
    }
}