using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Fixed version of BinaryLauncher with proper rendering and text handling
    /// </summary>
    public class FixedBinaryLauncher : GridLayout
    {
        private readonly BinariesManager _binariesManager;
        private readonly ConsolePanel _consolePanel;
        private readonly ListBox _binariesList;
        private readonly ImprovedLabel _detailsLabel;
        private readonly Button _launchButton;
        private readonly Button _refreshButton;
        private readonly TextBox _searchBox;
        private readonly ImprovedLabel _categoryLabel;
        private readonly ImprovedLabel _titleLabel;
        
        private Dictionary<string, List<BinaryConfiguration>> _binariesByCategory;
        private string _currentCategory = "All";
        private BinaryConfiguration _selectedBinary;
        private List<BinaryConfiguration> _filteredBinaries = new();
        private string _cachedDetailsText = string.Empty;
        private bool _initialized = false;
        
        public FixedBinaryLauncher(string id, BinariesManager binariesManager, ConsolePanel consolePanel) 
            : base(id)
        {
            _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
            _consolePanel = consolePanel ?? throw new ArgumentNullException(nameof(consolePanel));
            
            TuiLogger.LogInfo($"FixedBinaryLauncher {id} constructor started");
            
            // Basic dimension initialization
            Width = 60;
            Height = 20;
            
            BackgroundColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
            Padding = 1;
            HorizontalSpacing = 1;
            VerticalSpacing = 0;
            
            // Simple grid: 5 rows, 12 columns
            // Row 0: Title (1)
            // Row 1: Search and buttons (1)
            // Row 2: Binary list (star)
            // Row 3: Details (star)
            // Row 4: Launch button (2)
            
            // Add row definitions
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 1 });    // Title
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 1 });    // Search
            AddRow(new GridDefinition { Type = GridUnitType.Star, Value = 2 });     // List (2 parts)
            AddRow(new GridDefinition { Type = GridUnitType.Star, Value = 1 });     // Details (1 part)
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 2 });    // Launch button (height 2)
            
            // 12 equal columns
            for (int i = 0; i < 12; i++)
            {
                AddColumn(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
            }
            
            // Subscribe to configuration changes
            _binariesManager.BinariesChanged += OnBinariesChanged;
            
            // Create components
            _titleLabel = new ImprovedLabel("titleLabel")
            {
                Text = "📦 Binary Launcher",
                Colors = new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue),
                TextAlignment = TextAlignment.Left
            };
            
            _searchBox = new TextBox("searchBox")
            {
                Placeholder = "Search...",
                MaxLength = 50
            };
            
            _refreshButton = new Button("refreshButton")
            {
                Text = "⟳",
                OnClick = RefreshBinaries,
                NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkCyan),
                Width = 3,
                Height = 1
            };
            
            _categoryLabel = new ImprovedLabel("categoryLabel")
            {
                Text = "All",
                Colors = new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue),
                TextAlignment = TextAlignment.Right
            };
            
            _binariesList = new ListBox("binariesList")
            {
                OnSelectionChanged = OnBinarySelected,
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue),
                SelectedColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.Cyan)
            };
            
            _detailsLabel = new ImprovedLabel("detailsLabel")
            {
                Text = "Select a binary to view details",
                Colors = new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue)
            };
            
            _launchButton = new Button("launchButton")
            {
                Text = "🚀 LAUNCH",
                OnClick = LaunchSelectedBinary,
                NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.Green),
                Width = 12,
                Height = 2
            };
            
            // Add components to grid
            AddChild(_titleLabel);
            SetChildPosition(_titleLabel, 0, 0, 8, 1);
            
            AddChild(_searchBox);
            SetChildPosition(_searchBox, 0, 1, 7, 1);
            
            AddChild(_refreshButton);
            SetChildPosition(_refreshButton, 7, 1, 1, 1);
            
            AddChild(_categoryLabel);
            SetChildPosition(_categoryLabel, 8, 1, 4, 1);
            
            AddChild(_binariesList);
            SetChildPosition(_binariesList, 0, 2, 12, 1);
            
            AddChild(_detailsLabel);
            SetChildPosition(_detailsLabel, 0, 3, 12, 1);
            
            AddChild(_launchButton);
            SetChildPosition(_launchButton, 6, 4, 4, 1); // Centered
            
            // Load initial data
            RefreshBinaries();
            
            // Perform layout
            DoLayout();
            _initialized = true;
            
            TuiLogger.LogInfo($"FixedBinaryLauncher {Id} initialized");
        }
        
        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible || !_initialized) return;
            
            // Draw background
            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            
            // Draw single border
            renderEngine.DrawBox(absX, absY, Width, Height, BorderStyle.Single, 
                new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.DarkBlue));
            
            // Draw children
            base.Render(renderEngine);
        }
        
        public bool HandleInput(InputEvent inputEvent)
        {
            // Let children handle input first
            foreach (var child in Children)
            {
                if (child is IFocusable focusable && focusable.HasFocus && focusable.HandleInput(inputEvent))
                {
                    return true;
                }
            }
            
            // Handle global shortcuts
            if (inputEvent.Key == ConsoleKey.F && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                _searchBox.HasFocus = true;
                return true;
            }
            
            if (inputEvent.Key == ConsoleKey.Enter && _binariesList.HasFocus && _selectedBinary != null)
            {
                LaunchSelectedBinary();
                return true;
            }
            
            if (inputEvent.Key == ConsoleKey.LeftArrow && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                NavigateCategory(-1);
                return true;
            }
            
            if (inputEvent.Key == ConsoleKey.RightArrow && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                NavigateCategory(1);
                return true;
            }
            
            return false;
        }
        
        private void NavigateCategory(int direction)
        {
            if (_binariesByCategory == null || _binariesByCategory.Count == 0)
                return;

            var categories = _binariesByCategory.Keys.ToList();
            if (categories.Count == 0)
                return;

            int currentIndex = categories.IndexOf(_currentCategory);
            if (currentIndex == -1)
                currentIndex = 0;

            int newIndex = (currentIndex + direction) % categories.Count;
            if (newIndex < 0)
                newIndex = categories.Count - 1;

            _currentCategory = categories[newIndex];
            _categoryLabel.Text = _currentCategory;
            UpdateFilteredBinaries();
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
                
                TuiLogger.LogInfo($"Refreshed binaries: {allBinaries.Count} total");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to refresh binaries", ex);
                _consolePanel.AddOutputLine($"Error refreshing binaries: {ex.Message}", true);
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
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _filteredBinaries = categoryBinaries;
                }
                else
                {
                    _filteredBinaries = categoryBinaries
                        .Where(b => 
                            (b.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (b.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                        .ToList();
                }
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

            // Clear selection if current selection is not in filtered list
            if (_selectedBinary != null && !_filteredBinaries.Contains(_selectedBinary))
            {
                _selectedBinary = null;
                UpdateDetailsLabel();
            }

            _categoryLabel.Text = $"{_currentCategory} ({_filteredBinaries.Count})";
            _titleLabel.Text = $"📦 Binary Launcher ({_filteredBinaries.Count})";
        }
        
        private void OnBinarySelected(int index)
        {
            if (index >= 0 && index < _filteredBinaries.Count)
            {
                _selectedBinary = _filteredBinaries[index];
                UpdateDetailsLabel();
            }
        }
        
        private void UpdateDetailsLabel()
        {
            if (_selectedBinary == null)
            {
                var newText = "Select a binary to view details";
                if (_cachedDetailsText != newText)
                {
                    _cachedDetailsText = newText;
                    _detailsLabel.Text = newText;
                    TuiLogger.LogDebug($"DetailsLabel updated: {newText}");
                }
                return;
            }

            var details = new System.Text.StringBuilder();
            details.AppendLine($"{_selectedBinary.Icon} {_selectedBinary.Name}");
            details.AppendLine($"Path: {_selectedBinary.ExecutablePath}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.Description))
                details.AppendLine($"Desc: {_selectedBinary.Description}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.Arguments))
                details.AppendLine($"Args: {_selectedBinary.Arguments}");

            var newDetailsText = details.ToString();
            
            // Update only if text changed
            if (_cachedDetailsText != newDetailsText)
            {
                _cachedDetailsText = newDetailsText;
                _detailsLabel.Text = newDetailsText;
                TuiLogger.LogDebug($"DetailsLabel updated for: {_selectedBinary.Name}");
            }
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
                    _consolePanel.AddOutputLine($"⚙️ Args: {_selectedBinary.Arguments}");

                _consolePanel.StartProcess(
                    _selectedBinary.ExecutablePath,
                    _selectedBinary.Arguments,
                    _selectedBinary.WorkingDirectory);
                
                TuiLogger.LogInfo($"Launched: {_selectedBinary.Name}");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"Failed to launch: {ex.Message}", true);
                TuiLogger.LogError($"Launch failed for {_selectedBinary.Name}", ex);
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