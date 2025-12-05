using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Modern binary launcher using SimpleGridLayout and ComponentFactory
    /// </summary>
    public class BinaryLauncherNew : SimpleGridLayout
    {
        private readonly BinariesManager _binariesManager;
        private readonly ConsolePanel _consolePanel;
        
        private ITextBox _searchBox;
        private IButton _refreshButton;
        private IListBox _binariesList;
        private ILabel _detailsLabel;
        private IButton _launchButton;
        private ILabel _titleLabel;
        private ILabel _statusLabel;
        
        private List<BinaryConfiguration> _filteredBinaries = new();
        private BinaryConfiguration _selectedBinary;
        private string _currentCategory = "All";
        private Dictionary<string, List<BinaryConfiguration>> _binariesByCategory;

        /// <summary>
        /// Initializes a new BinaryLauncherNew
        /// </summary>
        public BinaryLauncherNew(string id, BinariesManager binariesManager, ConsolePanel consolePanel) 
            : base(id)
        {
            _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
            _consolePanel = consolePanel ?? throw new ArgumentNullException(nameof(consolePanel));
            
            TuiLogger.LogInfo($"BinaryLauncherNew {id} initializing");
            
            // Configure grid layout (3x3 grid)
            Columns = 3;
            Rows = 4;
            Width = 80;
            Height = 25;
            BackgroundColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
            Padding = 1;
            HorizontalSpacing = 1;
            VerticalSpacing = 0;
            
            InitializeComponents();
            LayoutComponents();
            
            // Subscribe to events
            _binariesManager.BinariesChanged += OnBinariesChanged;
            
            // Load initial data
            RefreshBinaries();
            
            TuiLogger.LogInfo($"BinaryLauncherNew {Id} initialized");
        }
        
        private void InitializeComponents()
        {
            // Create components using ComponentFactory
            _titleLabel = ComponentFactory.CreateLabel("title", "🚀 Binary Launcher")
                .WithColors(new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue));
            
            _searchBox = ComponentFactory.CreateTextBox("search", "Search binaries...")
                .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.White));
            
            _refreshButton = ComponentFactory.CreateButton("refresh", "⟳", RefreshBinaries)
                .WithSize(3, 1)
                .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.Cyan));
            
            _binariesList = ComponentFactory.CreateListBox("binaries")
                .WithSelectionHandler(OnBinarySelected);
            
            _detailsLabel = ComponentFactory.CreateLabel("details", "Select a binary to view details")
                .WithColors(new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue));
            
            _launchButton = ComponentFactory.CreateButton("launch", "🚀 LAUNCH", LaunchSelectedBinary)
                .WithSize(12, 2)
                .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.Green));
            
            _statusLabel = ComponentFactory.CreateLabel("status", "Ready")
                .WithColors(new ColorScheme(ConsoleColor.Gray, ConsoleColor.DarkBlue));
        }
        
        private void LayoutComponents()
        {
            // Row 0: Title (spans 3 columns)
            AddChild(_titleLabel, 0, 0, 3, 1);
            
            // Row 1: Search and Refresh
            AddChild(_searchBox, 0, 1, 2, 1);
            AddChild(_refreshButton, 2, 1, 1, 1);
            
            // Row 2: Binaries list (spans 3 columns, 2 rows tall)
            AddChild(_binariesList, 0, 2, 3, 2);
            
            // Row 4: Details (spans 3 columns)
            AddChild(_detailsLabel, 0, 4, 3, 1);
            
            // Row 5: Launch button (centered)
            AddChild(_launchButton, 1, 5, 1, 1);
            
            // Row 6: Status
            AddChild(_statusLabel, 0, 6, 3, 1);
        }
        
        /// <summary>
        /// Handles input events for the launcher
        /// </summary>
        public bool HandleInput(InputEvent inputEvent)
        {
            // Handle global shortcuts
            if (inputEvent.Key == ConsoleKey.F && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                if (_searchBox is IFocusable focusableSearch)
                {
                    focusableSearch.HasFocus = true;
                    return true;
                }
            }
            
            if (inputEvent.Key == ConsoleKey.Enter && _selectedBinary != null)
            {
                LaunchSelectedBinary();
                return true;
            }
            
            if (inputEvent.Key == ConsoleKey.R && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                RefreshBinaries();
                return true;
            }
            
            return false;
        }
        
        private void OnBinariesChanged(object sender, BinariesChangedEventArgs e)
        {
            RefreshBinaries();
        }
        
        private void RefreshBinaries()
        {
            try
            {
                _binariesManager.LoadConfiguration();
                _binariesByCategory = _binariesManager.GetBinariesByCategory();
                
                // Add "All" category
                var allBinaries = _binariesManager.GetEnabledBinaries();
                _binariesByCategory["All"] = allBinaries;
                
                UpdateFilteredBinaries();
                UpdateStatus($"Loaded {allBinaries.Count} binaries");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to refresh binaries", ex);
                _consolePanel.AddOutputLine($"Error: {ex.Message}", true);
                UpdateStatus("Error loading binaries");
            }
        }
        
        private void UpdateFilteredBinaries()
        {
            var searchTerm = _searchBox.Text.Trim();
            
            if (_currentCategory == "All")
            {
                _filteredBinaries = _binariesManager.SearchBinaries(searchTerm);
            }
            else if (_binariesByCategory.TryGetValue(_currentCategory, out var categoryBinaries))
            {
                _filteredBinaries = categoryBinaries
                    .Where(b => string.IsNullOrEmpty(searchTerm) ||
                        (b.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (b.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }
            else
            {
                _filteredBinaries = new List<BinaryConfiguration>();
            }
            
            // Update list box
            _binariesList.Items.Clear();
            foreach (var binary in _filteredBinaries)
            {
                _binariesList.Items.Add($"{binary.Icon} {binary.Name}");
            }
            
            // Update title with count
            _titleLabel.Text = $"🚀 Binary Launcher ({_filteredBinaries.Count})";
            
            // Clear selection if current selection not in filtered list
            if (_selectedBinary != null && !_filteredBinaries.Contains(_selectedBinary))
            {
                _selectedBinary = null;
                UpdateDetails();
            }
        }
        
        private void OnBinarySelected(int index)
        {
            if (index >= 0 && index < _filteredBinaries.Count)
            {
                _selectedBinary = _filteredBinaries[index];
                UpdateDetails();
            }
        }
        
        private void UpdateDetails()
        {
            if (_selectedBinary == null)
            {
                _detailsLabel.Text = "Select a binary to view details";
                _launchButton.Text = "🚀 LAUNCH";
                return;
            }
            
            var details = new System.Text.StringBuilder();
            details.AppendLine($"{_selectedBinary.Icon} {_selectedBinary.Name}");
            details.AppendLine($"Path: {Path.GetFileName(_selectedBinary.ExecutablePath)}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.Description))
            {
                details.AppendLine($"Desc: {_selectedBinary.Description}");
            }
            
            _detailsLabel.Text = details.ToString();
            _launchButton.Text = $"🚀 LAUNCH {_selectedBinary.Name}";
        }
        
        private void LaunchSelectedBinary()
        {
            if (_selectedBinary == null)
            {
                _consolePanel.AddOutputLine("Please select a binary first", true);
                return;
            }

            try
            {
                if (!_selectedBinary.Validate(out var errors))
                {
                    _consolePanel.AddOutputLine($"Validation failed for {_selectedBinary.Name}:", true);
                    foreach (var error in errors.Take(3))
                    {
                        _consolePanel.AddOutputLine($"  • {error}", true);
                    }
                    return;
                }

                _consolePanel.AddOutputLine($"🚀 Launching: {_selectedBinary.Name}");
                
                if (!string.IsNullOrEmpty(_selectedBinary.Arguments))
                {
                    _consolePanel.AddOutputLine($"⚙️ Args: {_selectedBinary.Arguments}");
                }

                _consolePanel.StartProcess(
                    _selectedBinary.ExecutablePath,
                    _selectedBinary.Arguments,
                    _selectedBinary.WorkingDirectory);
                
                UpdateStatus($"Launched: {_selectedBinary.Name}");
                TuiLogger.LogInfo($"Launched: {_selectedBinary.Name}");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"Failed to launch: {ex.Message}", true);
                UpdateStatus($"Error: {ex.Message}");
                TuiLogger.LogError($"Launch failed for {_selectedBinary.Name}", ex);
            }
        }
        
        private void UpdateStatus(string message)
        {
            _statusLabel.Text = $"{DateTime.Now:HH:mm:ss} | {message}";
        }
        
        /// <summary>
        /// Renders the launcher with enhanced visuals
        /// </summary>
        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;
            
            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            // Draw main background
            renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            
            // Draw border
            renderEngine.DrawBox(absX, absY, Width, Height, BorderStyle.Double,
                new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.DarkBlue));
            
            // Draw grid lines for visualization (optional)
            DrawGridLines(renderEngine);
            
            // Render children
            base.Render(renderEngine);
        }
        
        private void DrawGridLines(IRenderEngine renderEngine)
        {
            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            var gridColor = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.DarkBlue);
            
            // Draw vertical grid lines
            for (int col = 1; col < Columns; col++)
            {
                int x = absX + (col * (Width / Columns));
                for (int y = absY; y < absY + Height; y++)
                {
                    renderEngine.DrawChar(x, y, '│', gridColor);
                }
            }
            
            // Draw horizontal grid lines
            for (int row = 1; row < Rows; row++)
            {
                int y = absY + (row * (Height / Rows));
                for (int x = absX; x < absX + Width; x++)
                {
                    renderEngine.DrawChar(x, y, '─', gridColor);
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _binariesManager.BinariesChanged -= OnBinariesChanged;
            }
            base.Dispose(disposing);
        }
    }
}