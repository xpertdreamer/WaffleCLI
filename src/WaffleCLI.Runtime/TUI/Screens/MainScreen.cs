using System.Drawing;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI;
using WaffleCLI.Runtime.TUI.Elements;

namespace WaffleCLI.Runtime.TUI.Screens;

/// <summary>
/// Represents the main screen of the WaffleCLI Text User Interface application.
/// </summary>
/// <remarks>
/// Provides a comprehensive TUI interface with command list view, output text view, and command input field.
/// Supports keyboard navigation, command execution, and real-time output display.
/// </remarks>
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
    
    /// <summary>
    /// Gets the title of the main screen.
    /// </summary>
    public string Title => "WaffleCLI TUI";

    /// <summary>
    /// Initializes a new instance of the <see cref="MainScreen"/> class.
    /// </summary>
    /// <param name="commandRegistry">The command registry for retrieving available commands.</param>
    /// <param name="commandExecutor">The command executor for running commands.</param>
    /// <param name="logger">The logger for recording screen events and errors.</param>
    public MainScreen(ICommandRegistry commandRegistry, ICommandExecutor commandExecutor, ILogger<MainScreen> logger)
    {
        _commandRegistry = commandRegistry;
        _commandExecutor = commandExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the main screen and its UI elements asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    /// <remarks>
    /// Sets up the command list view, output text view, and command input field with proper positioning and event handlers.
    /// Loads available commands from the registry and displays initialization information.
    /// </remarks>
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
        _outputTextView.AppendLine($"Loaded {_commands.Count} commands");
        _outputTextView.AppendLine("Use Tab to navigate, Up/Down Arrows to select, Enter to execute");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Renders the main screen and all its UI elements asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous rendering operation.</returns>
    /// <remarks>
    /// Clears the console, draws the header and footer with usage instructions, and renders all UI elements.
    /// Maintains proper color management and cursor positioning.
    /// </remarks>
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

    /// <summary>
    /// Handles keyboard input for the main screen asynchronously.
    /// </summary>
    /// <param name="keyInfo">The keyboard input information.</param>
    /// <returns>A task that represents the asynchronous key handling operation.</returns>
    /// <remarks>
    /// Supports application exit with Ctrl+Q, element navigation with Tab, and delegates key handling
    /// to the currently focused UI element. Triggers re-rendering when key handling results in visual changes.
    /// </remarks>
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
            return RenderAsync();
        }

        var focusedElement = _elements[_focusedElementIndex];
        if (focusedElement.HandleKey(keyInfo))
        {
            return RenderAsync();
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the focus state of all UI elements based on the current focused element index.
    /// </summary>
    /// <remarks>
    /// Ensures only one text field has focus at a time and updates visual focus indicators accordingly.
    /// </remarks>
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

    /// <summary>
    /// Handles command selection from the commands list view.
    /// </summary>
    /// <param name="index">The index of the selected command in the commands list.</param>
    /// <remarks>
    /// Populates the command text field with the selected command name and transfers focus to the text field.
    /// </remarks>
    private void OnCommandSelected(int index)
    {
        if (index < 0 || index >= _commands.Count) return;
        var command = _commands[index];
        _commandTextField.Text = command.Name;
        _focusedElementIndex = _elements.IndexOf(_commandTextField);
        UpdateFocus();
    }

    /// <summary>
    /// Handles command submission from the command text field.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <remarks>
    /// Executes the submitted command, captures its output, and displays the results in the output text view.
    /// Handles both successful command execution and errors with appropriate logging and user feedback.
    /// </remarks>
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