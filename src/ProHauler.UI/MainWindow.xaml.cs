using System.Windows;
using ProHauler.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ProHauler.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Views.PerformanceWindow? _performanceWindow;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void PerformanceTracker_Click(object sender, RoutedEventArgs e)
    {
        // Check if performance window is already open
        if (_performanceWindow != null && _performanceWindow.IsLoaded)
        {
            // Bring existing window to front
            _performanceWindow.Activate();
            return;
        }

        // Get PerformanceWindow from DI container
        var app = Application.Current as App;
        if (app == null) return;

        var serviceProvider = app.Services;
        if (serviceProvider == null) return;

        _performanceWindow = serviceProvider.GetRequiredService<Views.PerformanceWindow>();

        // Position window next to MainWindow
        _performanceWindow.Left = this.Left + this.ActualWidth + 10;
        _performanceWindow.Top = this.Top;

        // Handle window closed event to clear reference
        _performanceWindow.Closed += (s, args) => _performanceWindow = null;

        _performanceWindow.Show();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Dispose ViewModel if it implements IDisposable
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}