using WaffleCLI.Abstractions.TUI.Components;
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
                    Console.WriteLine("  cl /EHsc /Fe:maze_persistent.exe main_persistent.cpp maze.cpp astar.cpp racemode.cpp");
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
    /// Maze Demo Application with persistent state support and Race Mode
    /// </summary>
    public class MazeDemoApp : WaffleCLI.Core.TUI.Components.Layout.SimpleGridLayout
    {
        private ConsolePanel _consolePanel;
        private MazeActionListBox _actionList;
        private MazeActionListBox _raceActionList;
        private ILabel _statusLabel;
        private ILabel _titleLabel;
        private ILabel _controlLegend;
        private IButton _clearConsoleButton;
        private ILabel _raceStatusLabel;
        private IButton _raceUpButton;
        private IButton _raceDownButton;
        private IButton _raceLeftButton;
        private IButton _raceRightButton;
        private IButton _raceStartButton;
        private IButton _raceResetButton;
        
        private string _currentMazeFile = "maze_temp.txt";
        private bool _mazeLoaded = false;
        private bool _raceActive = false;
        private string _mazeExe = "maze_persistent.exe";
        private string _lastCommand = "";
        private IProcessRunner _currentProcess = null;

        public MazeDemoApp() : base("mazeDemoApp")
        {
            // Configure grid: 12 columns, 12 rows
            Columns = 12;
            Rows = 12;
            Width = 120;
            Height = 45;
            BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
            Padding = 1;
            HorizontalSpacing = 1;
            VerticalSpacing = 0;

            InitializeComponents();
            LayoutComponents();
            
            // Start monitoring race status
            CheckRaceStatus();
        }

        private void InitializeComponents()
        {
            // Create title label
            _titleLabel = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("title", 
                    "🔍 Maze Path Finder (Persistent) CourseWork")
                .WithAlignment(TextAlignment.Center)
                .WithColors(new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue))
                .Build();

            // Create main action list box
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
            _actionList.SetExecuteHandler(ExecuteMainAction);

            // Create race action list box
            _raceActionList = new MazeActionListBox("raceActions")
            {
                Items = new System.Collections.ArrayList(new[] 
                {
                    "🏁 Start Race Mode",
                    "🔄 Reset Race",
                    "📊 Show Race State",
                    "↑ Move Up",
                    "↓ Move Down",
                    "← Move Left",
                    "→ Move Right",
                    "📈 Show Race Results"
                }),
                Width = 25,
                Height = 9,
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue),
                SelectedColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.Green),
                SelectedFocusColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.White)
            };
            _raceActionList.SetExecuteHandler(ExecuteRaceAction);

            // Create console panel
            _consolePanel = new ConsolePanel("console")
            {
                Width = 70,
                Height = 25,
                Prompt = "> ",
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.Black),
                InputLine = false
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

            // Create race status label
            _raceStatusLabel = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("raceStatus", 
                    "Race: Not Active")
                .WithColors(new ColorScheme(ConsoleColor.Red, ConsoleColor.DarkBlue))
                .Build();

            // Create control legend label at the bottom
            _controlLegend = WaffleCLI.Core.TUI.Components.ComponentFactory.CreateLabel("legend",
                    "CONTROLS: ↑↓ Navigate Lists • Enter Execute • Tab Next • Shift+Tab Previous • Esc Exit")
                .WithAlignment(TextAlignment.Center)
                .WithColors(new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue))
                .Build();

            // Initial console messages
            _consolePanel.AddOutputLine("✅ Maze Path Finder Demo Initialized");
            
            if (!File.Exists(_mazeExe))
            {
                _consolePanel.AddOutputLine($"❌ ERROR: {_mazeExe} not found!", true);
                _consolePanel.AddOutputLine("Please compile persistent version:", true);
                _consolePanel.AddOutputLine("  cl /EHsc /Fe:maze_persistent.exe main_persistent.cpp maze.cpp astar.cpp racemode.cpp", true);
                _statusLabel.Text = "Status: Error - maze_persistent.exe not found";
            }
            else
            {
                _consolePanel.AddOutputLine($"📁 Maze executable: {_mazeExe}");
                _consolePanel.AddOutputLine("💾 State is preserved between operations");
                _consolePanel.AddOutputLine("🎮 Race Mode available when maze is loaded");
                _consolePanel.AddOutputLine("👉 Select an action and press Enter to execute");
                
                // Check if maze already exists from previous run
                CheckMazeFileStatus();
            }
        }

        private void LayoutComponents()
        {
            // Row 0: Title (spans all 12 columns)
            AddChild(_titleLabel as IComponent, 0, 0, 12, 1);
            
            // Row 1-4: Action list (left) and Race actions (center)
            AddChild(_actionList as IComponent, 0, 1, 4, 4);
            
            // Row 5: Status labels
            AddChild(_statusLabel as IComponent, 0, 5, 6, 1);
            AddChild(_raceStatusLabel as IComponent, 6, 5, 6, 1);
            
            // Row 6: Buttons
            AddChild(_clearConsoleButton as IComponent, 0, 6, 2, 1);
            
            // Row 11: Control legend (spans all 12 columns)
            AddChild(_controlLegend as IComponent, 0, 11, 12, 1);
            
            // Add console panel last to ensure it's on top
            AddChild(_consolePanel, 7, 1, 5, 5);
            
            AddChild(_raceActionList as IComponent, 4, 1, 3, 4);
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
                UpdateRaceButtonsState();
            }
            else
            {
                _mazeLoaded = false;
                TuiLogger.LogInfo("No existing maze file found");
                UpdateRaceButtonsState();
            }
        }

        /// <summary>
        /// Check race status file and update UI
        /// </summary>
        private void CheckRaceStatus()
        {
            bool raceFileExists = File.Exists("race_active.tmp");
            _raceActive = raceFileExists;
            
            if (_raceActive)
            {
                _raceStatusLabel.Text = "Race: ACTIVE";
                // _raceStatusLabel.Colors = new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue);
            }
            else
            {
                _raceStatusLabel.Text = "Race: Not Active";
                // _raceStatusLabel.Colors = new ColorScheme(ConsoleColor.Red, ConsoleColor.DarkBlue);
            }
            
            UpdateRaceButtonsState();
        }

        /// <summary>
        /// Update race buttons enabled state based on conditions
        /// </summary>
        private void UpdateRaceButtonsState()
        {
            bool canStartRace = _mazeLoaded && !_raceActive;
            bool canMove = _mazeLoaded && _raceActive;
            
            // Update list items colors
            for (int i = 0; i < _raceActionList.Items.Count; i++)
            {
                bool isEnabled = true;
                
                switch (i)
                {
                    case 0: // Start Race Mode
                        isEnabled = canStartRace;
                        break;
                    case 1: // Reset Race
                    case 2: // Show Race State
                        isEnabled = _raceActive;
                        break;
                    case 3: // Move Up
                    case 4: // Move Down
                    case 5: // Move Left
                    case 6: // Move Right
                        isEnabled = canMove;
                        break;
                    case 7: // Show Race Results
                        isEnabled = File.Exists("race_results.txt");
                        break;
                }
                
                // We can't directly change item colors, but we can update the list
                // For now, we'll just update status messages
            }
        }

        private void ExecuteMainAction(int actionIndex)
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

        private void ExecuteRaceAction(int actionIndex)
        {
            if (!File.Exists(_mazeExe))
            {
                _consolePanel.AddOutputLine($"❌ Maze executable '{_mazeExe}' not found", true);
                _statusLabel.Text = "Status: Error - maze.exe not found";
                return;
            }

            // Check if maze is loaded for race commands
            if (!_mazeLoaded && actionIndex != 7) // Except for showing results
            {
                _consolePanel.AddOutputLine("❌ No maze loaded for race mode!", true);
                _consolePanel.AddOutputLine("ℹ️ Please generate or load a maze first", true);
                return;
            }

            // Stop any running process first
            _consolePanel.StopProcess();
            Thread.Sleep(50);

            try
            {
                _consolePanel.ClearOutput();
                string command = "";
                string actionName = "";

                switch (actionIndex)
                {
                    case 0: // Start Race Mode
                        if (_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ Race already active!", true);
                            return;
                        }
                        command = "race_start";
                        actionName = "Start Race";
                        break;

                    case 1: // Reset Race
                        if (!_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ No active race to reset!", true);
                            return;
                        }
                        command = "race_reset";
                        actionName = "Reset Race";
                        break;

                    case 2: // Show Race State
                        command = "race_state";
                        actionName = "Show Race State";
                        break;

                    case 3: // Move Up
                        if (!_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ Start race first!", true);
                            return;
                        }
                        command = "race_up";
                        actionName = "Move Up";
                        break;

                    case 4: // Move Down
                        if (!_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ Start race first!", true);
                            return;
                        }
                        command = "race_down";
                        actionName = "Move Down";
                        break;

                    case 5: // Move Left
                        if (!_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ Start race first!", true);
                            return;
                        }
                        command = "race_left";
                        actionName = "Move Left";
                        break;

                    case 6: // Move Right
                        if (!_raceActive)
                        {
                            _consolePanel.AddOutputLine("⚠️ Start race first!", true);
                            return;
                        }
                        command = "race_right";
                        actionName = "Move Right";
                        break;

                    case 7: // Show Race Results
                        if (!File.Exists("race_results.txt"))
                        {
                            _consolePanel.AddOutputLine("ℹ️ No race results file found", true);
                            _consolePanel.AddOutputLine("Complete a race first to see results", true);
                            return;
                        }
                        ShowRaceResults();
                        return;

                    default:
                        return;
                }

                // Store the command being executed
                _lastCommand = command;
                
                // Execute the command
                _consolePanel.AddOutputLine($"🎮 Executing Race Command: {command}");
                _statusLabel.Text = $"Status: {actionName}...";

                // Start process and monitor it
                _consolePanel.StartProcess(_mazeExe, command);
                
                TuiLogger.LogInfo($"Started race command: {command}");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"❌ Error: {ex.Message}", true);
                _statusLabel.Text = "Status: Error occurred";
                _lastCommand = "";
                TuiLogger.LogError("Failed to execute race command", ex);
            }
        }

        private void ShowRaceResults()
        {
            try
            {
                string results = File.ReadAllText("race_results.txt");
                _consolePanel.AddOutputLine("📊 RACE RESULTS:");
                _consolePanel.AddOutputLine("════════════════════════════════════");
                
                string[] lines = results.Split('\n');
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _consolePanel.AddOutputLine(line);
                    }
                }
                
                _consolePanel.AddOutputLine("════════════════════════════════════");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"❌ Error reading race results: {ex.Message}", true);
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Check if process completed and update race status
            if (!string.IsNullOrEmpty(_lastCommand))
            {
                // Check race status for race commands
                if (_lastCommand.StartsWith("race_"))
                {
                    // Delay a bit then check race status
                    Thread.Sleep(100);
                    CheckRaceStatus();
                    
                    // If it was a movement command, check if race finished
                    if (_lastCommand.EndsWith("_up") || _lastCommand.EndsWith("_down") || 
                        _lastCommand.EndsWith("_left") || _lastCommand.EndsWith("_right"))
                    {
                        // Check if race finished by looking for results file
                        if (File.Exists("race_results.txt"))
                        {
                            _statusLabel.Text = "Status: Race Completed!";
                            _raceActive = false;
                            UpdateRaceButtonsState();
                            
                            // Show results automatically
                            Thread.Sleep(500);
                            ShowRaceResults();
                        }
                    }
                    
                    _lastCommand = "";
                }
                else
                {
                    // For maze commands that should create/modify the maze file
                    bool shouldUpdateMazeStatus = _lastCommand is "gen" or "load" or "full";
                    
                    if (shouldUpdateMazeStatus)
                    {
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
                                    UpdateRaceButtonsState();
                                }
                            }
                        }
                    }
                    else if (_lastCommand == "current")
                    {
                        CheckMazeFileStatusDelayed();
                    }
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
            UpdateRaceButtonsState();
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
            
            // Draw main border
            renderEngine.DrawBox(absX, absY, Width, Height, 
                BorderStyle.Double,
                new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue));

            // Draw separator between action list and race list
            int separator1X = absX + (Width * 4 / 12);
            var separatorColor = new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.DarkBlue);
            
            for (int y = absY + 1; y < absY + Height - 2; y++)
            {
                renderEngine.DrawChar(separator1X, y, '│', separatorColor);
            }

            // Draw separator between race list and console
            int separator2X = absX + (Width * 7 / 12);
            for (int y = absY + 1; y < absY + Height - 2; y++)
            {
                renderEngine.DrawChar(separator2X, y, '│', separatorColor);
            }

            // Draw section headers
            renderEngine.DrawString(separator1X + 2, absY + 1, "🎮 MAZE ACTIONS", 
                new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue));
            renderEngine.DrawString(separator2X + 2, absY + 1, "🏁 RACE MODE", 
                new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue));
            renderEngine.DrawString(separator2X + (Width - separator2X) / 2, absY + 1, "📟 CONSOLE", 
                new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue));

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

            // Draw race status indicator
            if (_raceActive)
            {
                renderEngine.DrawString(absX + Width - 15, absY + 1, "🏁 ACTIVE", 
                    new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue));
            }

            // Render children
            base.Render(renderEngine);
        }
    }
}