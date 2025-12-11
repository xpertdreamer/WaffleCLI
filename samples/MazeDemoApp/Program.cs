using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Infrastructure.Logging;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Processes;

namespace MazeDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TuiLogger.EnableLogging = true;
            TuiLogger.QuietMode = true;
            TuiLogger.LogFile = "maze-demo.log";
            TuiLogger.ClearLog();

            TuiLogger.LogInfo("=== Maze Path Finder (Persistent) CourseWork ===");

            try
            {
                Console.WriteLine("🚀 Initializing Maze Path Finder Demo...");
                
                // Check for correct maze version
                string mazeExe = "maze_persistent.exe";
                if (!File.Exists(mazeExe))
                {
                    Console.WriteLine($"❌ ERROR: {mazeExe} not found!");
                    Console.WriteLine("Please compile the persistent version with:");
                    Console.WriteLine("  cl /EHsc /Fe:maze_persistent.exe main_persistent.cpp maze.cpp astar.cpp");
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }
                
                var app = new TuiApplicationBuilder()
                    .WithFrameRate(30)
                    .WithTheme("default")
                    .EnableDoubleBuffering(true)
                    .EnableInputLogging(false)
                    .UseRootComponent<MazeDemoApp>()
                    .Build();
                
                Console.WriteLine("✅ Application built!");
                Console.WriteLine($"📁 Using maze executable: {mazeExe}");
                Console.WriteLine("\n🎮 CONTROLS:");
                Console.WriteLine("   • ↑↓ Navigate List");
                Console.WriteLine("   • Enter Execute selected action");
                Console.WriteLine("   • Tab/Shift+Tab Navigate components");
                Console.WriteLine("   • Esc Exit");
                Console.WriteLine("\nPress any key to start...");
                
                Console.ReadKey(true);
                Console.Clear();
                
                app.Run();
                app.Shutdown();
                
                Console.WriteLine("👋 Demo finished.");
                Console.WriteLine("📋 Log: maze-demo.log");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Demo failed", ex);
                Console.Clear();
                Console.WriteLine($"💥 Error: {ex.Message}");
                Console.WriteLine($"📋 Check log: {Path.GetFullPath("maze-demo.log")}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// Custom ListBox for maze actions with separate selection and execution
    /// </summary>
    public class MazeActionListBox : WaffleCLI.Core.TUI.Components.Primitive.ListBox
    {
        private Action<int> _onExecuteAction;
        
        public MazeActionListBox(string id) : base(id)
        {
        }
        
        /// <summary>
        /// Sets the action to execute when Enter is pressed
        /// </summary>
        public void SetExecuteHandler(Action<int> onExecute)
        {
            _onExecuteAction = onExecute;
        }
        
        public override bool HandleInput(WaffleCLI.Abstractions.TUI.Input.InputEvent inputEvent)
        {
            // First let base class handle navigation (arrow keys, etc.)
            bool handled = base.HandleInput(inputEvent);
            
            // If Enter is pressed and we have a selection, execute the action
            if (inputEvent.Key == ConsoleKey.Enter && SelectedIndex >= 0 && _onExecuteAction != null)
            {
                _onExecuteAction(SelectedIndex);
                return true;
            }
            
            return handled;
        }
    }

    /// <summary>
    /// Maze Demo Application with persistent state support
    /// Executes actions only when Enter is pressed on selected item
    /// Includes control legend at the bottom
    /// Console panel without input line (display only)
    /// </summary>
    public class MazeDemoApp : WaffleCLI.Core.TUI.Components.Layout.SimpleGridLayout
    {
        private ConsolePanel _consolePanel;
        private MazeActionListBox _actionList;
        private ILabel _statusLabel;
        private ILabel _titleLabel;
        private ILabel _controlLegend;
        private IButton _clearConsoleButton;
        private string _currentMazeFile = "maze_temp.txt";
        private bool _mazeLoaded = false;
        private string _mazeExe = "maze_persistent.exe";
        private string _lastCommand = "";
        private IProcessRunner _currentProcess = null;

        public MazeDemoApp() : base("mazeDemoApp")
        {
            // Configure grid: 12 columns, 8 rows (7 for content, 1 for legend)
            Columns = 12;
            Rows = 8;
            Width = 120;
            Height = 35;
            BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
            Padding = 1;
            HorizontalSpacing = 1;
            VerticalSpacing = 0;

            InitializeComponents();
            LayoutComponents();
        }

        private void InitializeComponents()
        {
            // Create title label
            _titleLabel = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("title", 
                    "🔍 Maze Path Finder (Persistent) CourseWork")
                .WithAlignment(TextAlignment.Center)
                .WithColors(new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue))
                .Build();

            // Create custom action list box
            _actionList = new MazeActionListBox("actions")
            {
                Items = new System.Collections.ArrayList(new[] 
                {
                    "1. Generate New Maze",
                    "2. Load Maze from File",
                    "3. Save Current Maze As...",
                    "4. Find Path in Current Maze",
                    "5. Print Current Maze",
                    "6. Generate and Save Maze",
                    "7. Check Current Maze Status"
                }),
                Width = 40,
                Height = 8,
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue),
                SelectedColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.Cyan),
                SelectedFocusColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.White)
            };

            // Set execute handler
            _actionList.SetExecuteHandler(ExecuteAction);

            // Create console panel directly (not through factory) to access InputLine property
            _consolePanel = new ConsolePanel("console")
            {
                Width = 70,
                Height = 18,
                Prompt = "> ",
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.Black),
                InputLine = false // Disable input line - console is display only
            };

            // Create clear console button
            _clearConsoleButton = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateButton("clear", "🧹 CLEAR")
                .WithSize(10, 3)
                .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkYellow))
                .WithClickHandler(() => _consolePanel.ClearOutput())
                .Build();

            // Create status label
            _statusLabel = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("status", 
                    "Status: Ready")
                .WithColors(new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue))
                .Build();

            // Create control legend label at the bottom
            _controlLegend = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("legend",
                    "CONTROLS: ↑↓ Navigate List • Enter Execute • Tab Next • Shift+Tab Previous • Esc Exit")
                .WithAlignment(TextAlignment.Center)
                .WithColors(new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue))
                .Build();

            // Initial console messages
            _consolePanel.AddOutputLine("✅ Maze Path Finder Demo Initialized");
            
            if (!File.Exists(_mazeExe))
            {
                _consolePanel.AddOutputLine($"❌ ERROR: {_mazeExe} not found!", true);
                _consolePanel.AddOutputLine("Please compile persistent version:", true);
                _consolePanel.AddOutputLine("  cl /EHsc /Fe:maze_persistent.exe main_persistent.cpp maze.cpp astar.cpp", true);
                _statusLabel.Text = "Status: Error - maze_persistent.exe not found";
            }
            else
            {
                _consolePanel.AddOutputLine($"📁 Maze executable: {_mazeExe}");
                _consolePanel.AddOutputLine("💾 State is preserved between operations");
                _consolePanel.AddOutputLine("👉 Select an action and press Enter to execute");
                
                // Check if maze already exists from previous run
                CheckMazeFileStatus();
            }
        }

        private void LayoutComponents()
        {
            // Add components in correct Z-order (console last to avoid overlap)
            
            // Row 0: Title (spans all 12 columns)
            AddChild(_titleLabel as WaffleCLI.Abstractions.TUI.Components.IComponent, 0, 0, 12, 1);
            
            // Row 1-4: Action list (left) and Console (right)
            AddChild(_actionList as WaffleCLI.Abstractions.TUI.Components.IComponent, 0, 1, 4, 4);
            
            // Row 5: Buttons and status
            AddChild(_clearConsoleButton as WaffleCLI.Abstractions.TUI.Components.IComponent, 0, 5, 2, 1);
            AddChild(_statusLabel as WaffleCLI.Abstractions.TUI.Components.IComponent, 2, 5, 10, 1);
            
            // Row 6: Control legend (spans all 12 columns)
            AddChild(_controlLegend as WaffleCLI.Abstractions.TUI.Components.IComponent, 0, 6, 12, 1);
            
            // Add console panel last to ensure it's on top for input handling
            // Position: column 4, row 1, spans 8 columns, 4 rows height
            AddChild(_consolePanel, 4, 1, 8, 4);
        }

        /// <summary>
        /// Check if maze file exists and update status
        /// </summary>
        private void CheckMazeFileStatus()
        {
            if (File.Exists(_currentMazeFile))
            {
                _mazeLoaded = true;
                _consolePanel.AddOutputLine($"ℹ️ Found existing maze state: {_currentMazeFile}");
                TuiLogger.LogInfo("Existing maze file detected");
            }
            else
            {
                _mazeLoaded = false;
                TuiLogger.LogInfo("No existing maze file found");
            }
        }

        private void ExecuteAction(int actionIndex)
        {
            if (!File.Exists(_mazeExe))
            {
                _consolePanel.AddOutputLine($"❌ Maze executable '{_mazeExe}' not found", true);
                _statusLabel.Text = "Status: Error - maze.exe not found";
                return;
            }

            // Stop any running process first
            _consolePanel.StopProcess();
            Thread.Sleep(50);

            try
            {
                _consolePanel.ClearOutput();
                string command = "";
                string args = "";
                bool needsInput = false;
                string inputPrompt = "";
                string defaultValue = "";
                string actionName = "";

                switch (actionIndex)
                {
                    case 0: // Generate New Maze
                        command = "gen";
                        actionName = "Generate New Maze";
                        needsInput = true;
                        inputPrompt = "Enter rows and cols (e.g., 10 15):";
                        defaultValue = "10 15";
                        break;

                    case 1: // Load Maze from File
                        command = "load";
                        actionName = "Load Maze from File";
                        needsInput = true;
                        inputPrompt = "Enter filename:";
                        defaultValue = "maze.txt";
                        break;

                    case 2: // Save Current Maze As...
                        if (!_mazeLoaded)
                        {
                            _consolePanel.AddOutputLine("❌ No maze loaded to save", true);
                            return;
                        }
                        command = "save";
                        actionName = "Save Current Maze";
                        needsInput = true;
                        inputPrompt = "Enter filename:";
                        defaultValue = "maze_saved.txt";
                        break;

                    case 3: // Find Path in Current Maze
                        if (!_mazeLoaded)
                        {
                            _consolePanel.AddOutputLine("❌ No maze loaded. Generate or load a maze first!", true);
                            _consolePanel.AddOutputLine("ℹ️ Use option 1 (Generate) or 2 (Load) first", true);
                            return;
                        }
                        command = "find";
                        actionName = "Find Path";
                        break;

                    case 4: // Print Current Maze
                        if (!_mazeLoaded)
                        {
                            _consolePanel.AddOutputLine("❌ No maze loaded to print", true);
                            return;
                        }
                        command = "print";
                        actionName = "Print Maze";
                        break;

                    case 5: // Generate and Save Maze
                        command = "full";
                        actionName = "Generate and Save Maze";
                        needsInput = true;
                        inputPrompt = "Enter rows, cols and filename (e.g., 10 15 maze.txt):";
                        defaultValue = "10 15 maze.txt";
                        break;

                    case 6: // Check Current Maze Status
                        command = "current";
                        actionName = "Check Maze Status";
                        break;

                    default:
                        return;
                }

                // Get input if needed
                if (needsInput)
                {
                    string input = ShowInputDialog($"Maze {command.ToUpper()}", inputPrompt, defaultValue);
                    if (string.IsNullOrEmpty(input))
                    {
                        _consolePanel.AddOutputLine("⚠️ Operation cancelled", true);
                        return;
                    }
                    args = input;
                }

                // Store the command being executed
                _lastCommand = command;
                
                // Execute the command
                _consolePanel.AddOutputLine($"🚀 Executing: {_mazeExe} {command} {args}");
                _statusLabel.Text = $"Status: Executing {actionName}...";

                // Start process and monitor it
                _consolePanel.StartProcess(_mazeExe, $"{command} {args}".Trim());
                
                TuiLogger.LogInfo($"Started maze command: {command} {args}");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"❌ Error: {ex.Message}", true);
                _statusLabel.Text = "Status: Error occurred";
                _lastCommand = "";
                TuiLogger.LogError("Failed to execute maze command", ex);
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Check if process completed by monitoring the maze file
            // This is a workaround since we can't directly hook into ConsolePanel's process events
            if (!string.IsNullOrEmpty(_lastCommand))
            {
                // For commands that should create/modify the maze file, check if process is done
                bool shouldUpdateMazeStatus = _lastCommand is "gen" or "load" or "full";
                
                if (shouldUpdateMazeStatus)
                {
                    // Simple heuristic: if maze file was recently modified, process likely completed
                    if (File.Exists(_currentMazeFile))
                    {
                        var fileInfo = new FileInfo(_currentMazeFile);
                        if ((DateTime.Now - fileInfo.LastWriteTime).TotalSeconds < 2)
                        {
                            if (!_mazeLoaded)
                            {
                                _mazeLoaded = true;
                                _consolePanel.AddOutputLine("✅ Maze loaded successfully");
                                _statusLabel.Text = "Status: Ready";
                                _lastCommand = "";
                                TuiLogger.LogInfo("Maze file updated, marked as loaded");
                            }
                        }
                    }
                }
                else if (_lastCommand == "current")
                {
                    // For status check, wait a bit then update
                    CheckMazeFileStatusDelayed();
                }
            }
        }

        private DateTime _lastStatusCheck = DateTime.MinValue;
        
        private void CheckMazeFileStatusDelayed()
        {
            // Only check once per second to avoid spamming
            if ((DateTime.Now - _lastStatusCheck).TotalSeconds < 1)
                return;
                
            _lastStatusCheck = DateTime.Now;
            
            if (File.Exists(_currentMazeFile))
            {
                if (!_mazeLoaded)
                {
                    _mazeLoaded = true;
                    _consolePanel.AddOutputLine($"✅ Maze state found: {_currentMazeFile}");
                }
            }
            else
            {
                if (_mazeLoaded)
                {
                    _mazeLoaded = false;
                    _consolePanel.AddOutputLine("ℹ️ No maze file found");
                }
            }
            
            _statusLabel.Text = "Status: Ready";
            _lastCommand = "";
        }

        private string ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            // Simple console-based input dialog
            Console.CursorVisible = true;
            
            // Position input at the bottom above the control legend
            int inputLine = Console.WindowHeight - 4;
            Console.SetCursorPosition(0, inputLine);
            
            // Clear input line
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, inputLine);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{title}: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{prompt} ");
            
            if (!string.IsNullOrEmpty(defaultValue))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{defaultValue}] ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            string? input = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            
            Console.CursorVisible = false;
            
            string result = string.IsNullOrEmpty(input) ? defaultValue : input;
            return result ?? "";
        }

        public override void Render(WaffleCLI.Abstractions.TUI.Rendering.IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            int absX = AbsoluteX;
            int absY = AbsoluteY;

            // Draw background
            renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            
            // Draw border
            renderEngine.DrawBox(absX, absY, Width, Height, 
                BorderStyle.Double,
                new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue));

            // Draw separator between list and console
            int separatorX = absX + (Width * 4 / 12);
            var separatorColor = new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.DarkBlue);

            for (int y = absY + 1; y < absY + Height - 2; y++) // Stop before legend
            {
                renderEngine.DrawChar(separatorX, y, '│', separatorColor);
            }

            // Draw separator line above control legend
            int legendSeparatorY = absY + Height - 2;
            var legendSeparatorColor = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.DarkBlue);
            for (int x = absX + 1; x < absX + Width - 1; x++)
            {
                renderEngine.DrawChar(x, legendSeparatorY, '─', legendSeparatorColor);
            }
            
            // Draw corners for legend separator
            renderEngine.DrawChar(absX, legendSeparatorY, '├', legendSeparatorColor);
            renderEngine.DrawChar(absX + Width - 1, legendSeparatorY, '┤', legendSeparatorColor);

            // Render children
            base.Render(renderEngine);
        }
    }
}