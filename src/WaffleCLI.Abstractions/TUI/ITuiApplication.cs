namespace WaffleCLI.Abstractions.TUI;

/// <summary>
/// Represents a Text User Interface (TUI) application that can run and manage TUI screens.
/// </summary>
/// <remarks>
/// TUI applications provide an interactive console-based user interface with screen management
/// and keyboard input handling capabilities.
/// </remarks>
public interface ITuiApplication
{
    /// <summary>
    /// Runs the TUI application asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the application.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method starts the main application loop, handles screen rendering, and processes user input.
    /// </remarks>
    Task RunAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the TUI application gracefully.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    /// This method should perform cleanup operations and ensure the application exits cleanly.
    /// </remarks>
    Task StopAsync();
}

/// <summary>
/// Represents a single screen in a Text User Interface application.
/// </summary>
/// <remarks>
/// TUI screens manage their own rendering logic, handle keyboard input, and maintain their state.
/// Multiple screens can be managed by a TUI application to create complex interactive interfaces.
/// </remarks>
public interface ITuiScreen
{
    /// <summary>
    /// Gets the title of the screen.
    /// </summary>
    /// <remarks>
    /// The title is typically displayed in the application header or window title area.
    /// </remarks>
    string Title { get; }

    /// <summary>
    /// Initializes the screen asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    /// <remarks>
    /// This method is called before the screen is first rendered and should be used for
    /// setting up initial state, loading data, or performing other initialization tasks.
    /// </remarks>
    Task InitializeAsync();

    /// <summary>
    /// Renders the screen content asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous rendering operation.</returns>
    /// <remarks>
    /// This method is responsible for drawing the screen's visual elements to the console.
    /// It may be called multiple times as the screen state changes or needs refreshing.
    /// </remarks>
    Task RenderAsync();

    /// <summary>
    /// Handles keyboard input asynchronously.
    /// </summary>
    /// <param name="keyInfo">The console key information containing the pressed key and modifiers.</param>
    /// <returns>A task that represents the asynchronous key handling operation.</returns>
    /// <remarks>
    /// This method is called when the user presses a key while this screen is active.
    /// Implementations should handle relevant keys and update screen state accordingly.
    /// </remarks>
    Task HandleKeyAsync(ConsoleKeyInfo keyInfo);
}