// File: WaffleCLI.Core.TUI/Components/Primitive/BinaryLauncher.cs

using System;
using System.Collections.Generic;
using System.Linq;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Processes;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Component for launching configured binaries
    /// </summary>
    public class BinaryLauncher : FocusableComponentBase
    {
        private readonly BinariesManager _binariesManager;
        private readonly ConsolePanel _consolePanel;
        private readonly ListBox _binariesList;
        private readonly Label _detailsLabel;
        private readonly Button _launchButton;
        private readonly Button _refreshButton;
        private readonly TextBox _searchBox;
        private readonly Label _categoryLabel;
        
        private Dictionary<string, List<BinaryConfiguration>> _binariesByCategory;
        private string _currentCategory = "All";
        private BinaryConfiguration _selectedBinary;
        private List<BinaryConfiguration> _filteredBinaries = new();

        public ColorScheme NormalColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.White);
        public ColorScheme CategoryColors { get; set; } = new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue);

        public BinaryLauncher(string id, BinariesManager binariesManager, ConsolePanel consolePanel) 
            : base(id)
        {
            _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
            _consolePanel = consolePanel ?? throw new ArgumentNullException(nameof(consolePanel));
            
            // Subscribe to configuration changes
            _binariesManager.BinariesChanged += OnBinariesChanged;
            
            // Default dimensions
            Width = 60;
            Height = 25;
            
            // Create child components
            _searchBox = new TextBox("searchBox")
            {
                X = 1,
                Y = 1,
                Width = 35,
                Height = 1,
                Placeholder = "Search binaries...",
                MaxLength = 100
            };
            
            _refreshButton = new Button("refreshButton")
            {
                X = 37,
                Y = 1,
                Width = 10,
                Height = 1,
                Text = "Refresh",
                OnClick = RefreshBinaries
            };
            
            _categoryLabel = new Label("categoryLabel")
            {
                X = 48,
                Y = 1,
                Width = 10,
                Height = 1,
                Text = "All",
                Colors = CategoryColors
            };
            
            _binariesList = new ListBox("binariesList")
            {
                X = 1,
                Y = 3,
                Width = 58,
                Height = 15,
                OnSelectionChanged = OnBinarySelected
            };
            
            _detailsLabel = new Label("detailsLabel")
            {
                X = 1,
                Y = 19,
                Width = 58,
                Height = 3,
                Text = "Select a binary to view details",
                Colors = new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue)
            };
            
            _launchButton = new Button("launchButton")
            {
                X = 1,
                Y = 22,
                Width = 15,
                Height = 3,
                Text = "🚀 Launch",
                OnClick = LaunchSelectedBinary,
                NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.Green)
            };
            
            // Add children
            AddChild(_searchBox);
            AddChild(_refreshButton);
            AddChild(_categoryLabel);
            AddChild(_binariesList);
            AddChild(_detailsLabel);
            AddChild(_launchButton);
            
            // Load initial data
            RefreshBinaries();
            
            TuiLogger.LogInfo($"BinaryLauncher {Id} initialized");
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            var colors = HasFocus ? FocusColors : NormalColors;
            var borderStyle = HasFocus ? BorderStyle.Double : BorderStyle.Single;
            
            // Draw background and border
            renderEngine.FillRectangle(X, Y, Width, Height, ' ', colors);
            renderEngine.DrawBox(X, Y, Width, Height, borderStyle, colors);
            
            // Draw title
            renderEngine.DrawString(X + 2, Y, $"📦 Binary Launcher ({_filteredBinaries.Count} available)", colors);
            
            // Render children
            base.Render(renderEngine);
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) return false;

            // Handle common navigation
            if (HandleCommonNavigation(inputEvent))
                return true;

            // Handle search shortcut (Ctrl+F)
            if (inputEvent.Key == ConsoleKey.F && 
                inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                SetFocusToSearch();
                return true;
            }

            // Handle launch shortcut (Enter when list has focus)
            if (inputEvent.Key == ConsoleKey.Enter && 
                _binariesList.HasFocus && 
                _selectedBinary != null)
            {
                LaunchSelectedBinary();
                return true;
            }

            // Handle category navigation (Ctrl+Left/Right)
            if (inputEvent.Key == ConsoleKey.LeftArrow && 
                inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                NavigateCategory(-1);
                return true;
            }
            
            if (inputEvent.Key == ConsoleKey.RightArrow && 
                inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
            {
                NavigateCategory(1);
                return true;
            }

            return false;
        }

        private void SetFocusToSearch()
        {
            _searchBox.HasFocus = true;
            _binariesList.HasFocus = false;
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
                _detailsLabel.Text = "Select a binary to view details";
            }

            _categoryLabel.Text = $"{_currentCategory} ({_filteredBinaries.Count})";
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
                _detailsLabel.Text = "Select a binary to view details";
                return;
            }

            var details = new System.Text.StringBuilder();
            details.AppendLine($"{_selectedBinary.Icon} {_selectedBinary.Name}");
            details.AppendLine($"📁 {_selectedBinary.ExecutablePath}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.Description))
                details.AppendLine($"📝 {_selectedBinary.Description}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.Arguments))
                details.AppendLine($"⚙️ Args: {_selectedBinary.Arguments}");
            
            if (!string.IsNullOrEmpty(_selectedBinary.WorkingDirectory))
                details.AppendLine($"📂 Dir: {_selectedBinary.WorkingDirectory}");

            _detailsLabel.Text = details.ToString();
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
                // Validate binary
                if (!_selectedBinary.Validate(out var errors))
                {
                    _consolePanel.AddOutputLine($"Binary validation failed:", true);
                    foreach (var error in errors)
                    {
                        _consolePanel.AddOutputLine($"  • {error}", true);
                    }
                    return;
                }

                _consolePanel.AddOutputLine($"🚀 Launching: {_selectedBinary.Name}");
                _consolePanel.AddOutputLine($"📁 Path: {_selectedBinary.ExecutablePath}");
                
                if (!string.IsNullOrEmpty(_selectedBinary.Arguments))
                    _consolePanel.AddOutputLine($"⚙️ Arguments: {_selectedBinary.Arguments}");

                // Start the process
                _consolePanel.StartProcess(
                    _selectedBinary.ExecutablePath,
                    _selectedBinary.Arguments,
                    _selectedBinary.WorkingDirectory);
                
                TuiLogger.LogInfo($"Launched binary: {_selectedBinary.Name} (ID: {_selectedBinary.Id})");
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"Failed to launch binary: {ex.Message}", true);
                TuiLogger.LogError($"Failed to launch binary {_selectedBinary.Name}", ex);
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            TuiLogger.LogInfo($"BinaryLauncher {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            TuiLogger.LogInfo($"BinaryLauncher {Id} lost focus");
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