using System.Windows;
using ProHauler.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ProHauler.UI.Views
{
    /// <summary>
    /// Interaction logic for PerformanceWindow.xaml
    /// </summary>
    public partial class PerformanceWindow : Window
    {
        private DebugWindow? _debugWindow;
        private bool _isDarkTheme = false;

        /// <summary>
        /// Initializes a new instance of the PerformanceWindow class.
        /// </summary>
        /// <param name="viewModel">The PerformanceViewModel to bind to this window.</param>
        public PerformanceWindow(PerformanceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));

            // Initialize the performance trend chart axes
            InitializePerformanceTrendChart();

            // Load light theme by default
            LoadTheme(false);
        }

        /// <summary>
        /// Initializes the performance trend chart with configured axes.
        /// TODO: Implement programmatic chart creation for LiveChartsCore
        /// </summary>
        private void InitializePerformanceTrendChart()
        {
            // Temporarily disabled - charts need to be created programmatically
            // and added to the Border containers (SmoothnessChartContainer, etc.)
            // This is a workaround for LiveChartsCore 2.0.0-rc6.1 XAML compilation issues with .NET 8
        }

        /// <summary>
        /// Loads the specified theme (light or dark).
        /// </summary>
        /// <param name="isDark">True for dark theme, false for light theme.</param>
        private void LoadTheme(bool isDark)
        {
            _isDarkTheme = isDark;

            // Clear existing theme resources
            Resources.MergedDictionaries.Clear();

            // Load the appropriate theme
            var themeUri = new System.Uri(
                isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml",
                System.UriKind.Relative);

            try
            {
                var theme = new ResourceDictionary { Source = themeUri };
                Resources.MergedDictionaries.Add(theme);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            LoadTheme(!_isDarkTheme);
        }

        /// <summary>
        /// Handles the Close menu item click event.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Debug Window menu item click event.
        /// </summary>
        private void DebugWindow_Click(object sender, RoutedEventArgs e)
        {
            // Check if debug window is already open
            if (_debugWindow != null && _debugWindow.IsLoaded)
            {
                _debugWindow.Activate();
                return;
            }

            // Get services from DI container
            var app = Application.Current as App;
            if (app?.Services == null) return;

            var telemetryClient = app.Services.GetRequiredService<ProHauler.Core.Interfaces.ITelemetryClient>();
            var performanceCalculator = app.Services.GetRequiredService<ProHauler.Core.Interfaces.IPerformanceCalculator>();

            _debugWindow = new DebugWindow(telemetryClient, performanceCalculator);
            _debugWindow.Closed += (s, args) => _debugWindow = null;
            _debugWindow.Show();
        }

        /// <summary>
        /// Handles the toggle breakdown button click event.
        /// </summary>
        private void ToggleBreakdown_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerformanceViewModel viewModel)
            {
                viewModel.IsBreakdownExpanded = !viewModel.IsBreakdownExpanded;
            }
        }

        /// <summary>
        /// Handles window size changes for responsive layout adjustments.
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Adjust layout based on window size
            if (e.NewSize.Width < 1000)
            {
                // Compact mode for smaller windows
                // Could adjust font sizes, margins, etc. if needed
            }
            else
            {
                // Normal mode for larger windows
            }
        }

        /// <summary>
        /// Cleanup when window is closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Dispose ViewModel if it implements IDisposable
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
