using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ProHauler.Core.Helpers;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ProHauler.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Performance Tracking window.
    /// Displays real-time driving performance metrics, session statistics, and trip history.
    /// </summary>
    public class PerformanceViewModel : ViewModelBase, IDisposable
    {
        private readonly IPerformanceCalculator _performanceCalculator;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITripRepository _tripRepository;
        private readonly ITripDetectionService _tripDetectionService;
        private readonly DispatcherTimer _notificationCleanupTimer;
        private readonly DispatcherTimer _tripCheckTimer;

        private PerformanceMetrics _currentMetrics;
        private ScoreBreakdown _scoreBreakdown;
        private string _connectionStatus = "Disconnected";
        private float _currentSpeed;
        private bool _isBreakdownExpanded = false;
        private ObservableCollection<PerformanceNotification> _recentNotifications;
        private bool _isLoading = false;
        private string _loadingMessage = "Loading...";

        // Trip history properties
        private ObservableCollection<Trip> _recentTrips;
        private TripStatistics _statistics;
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;

        // Sparkline chart series (initialized in InitializeSparklineCharts)
        private ISeries[] _smoothnessChartSeries = null!;
        private ISeries[] _speedComplianceChartSeries = null!;
        private ISeries[] _safetyChartSeries = null!;
        private ISeries[] _overallChartSeries = null!;

        // Performance trend chart series (initialized in InitializePerformanceTrendChart)
        private ISeries[] _performanceTrendSeries = null!;

        // Comparison bar chart series (initialized in InitializeComparisonChart)
        private ISeries[] _comparisonChartSeries = null!;

        // Comparison chart axes
        private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _comparisonXAxes = null!;
        private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _comparisonYAxes = null!;

        /// <summary>
        /// Initializes a new instance of the PerformanceViewModel class.
        /// </summary>
        /// <param name="performanceCalculator">The performance calculator service.</param>
        /// <param name="telemetryClient">The telemetry client service.</param>
        /// <param name="tripRepository">The trip repository for accessing trip history.</param>
        /// <param name="tripDetectionService">The trip detection service for monitoring trip lifecycle.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public PerformanceViewModel(
            IPerformanceCalculator performanceCalculator,
            ITelemetryClient telemetryClient,
            ITripRepository tripRepository,
            ITripDetectionService tripDetectionService)
        {
            _performanceCalculator = performanceCalculator ?? throw new ArgumentNullException(nameof(performanceCalculator));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            _tripDetectionService = tripDetectionService ?? throw new ArgumentNullException(nameof(tripDetectionService));

            // Initialize with default metrics
            _currentMetrics = _performanceCalculator.GetCurrentMetrics();
            _scoreBreakdown = _performanceCalculator.GetScoreBreakdown();
            _recentNotifications = new ObservableCollection<PerformanceNotification>();

            // Initialize trip history collections
            _recentTrips = new ObservableCollection<Trip>();
            _statistics = new TripStatistics();

            // Initialize commands
            FilterByDateCommand = new RelayCommand(async () => await FilterByDateAsync());
            ExportToCSVCommand = new RelayCommand(async () => await ExportToCSVAsync());

            // Subscribe to telemetry events
            _telemetryClient.DataUpdated += OnTelemetryDataUpdated;
            _telemetryClient.ConnectionStatusChanged += OnConnectionStatusChanged;
            _performanceCalculator.NotificationRaised += OnNotificationRaised;

            // Set up notification cleanup timer (runs every second)
            _notificationCleanupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _notificationCleanupTimer.Tick += CleanupOldNotifications;
            _notificationCleanupTimer.Start();

            // Set up trip check timer (runs every 5 seconds to check for new trips)
            _tripCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _tripCheckTimer.Tick += CheckForNewTrips;
            _tripCheckTimer.Start();

            // Initialize sparkline charts
            InitializeSparklineCharts();

            // Initialize performance trend chart
            InitializePerformanceTrendChart();

            // Initialize comparison chart
            InitializeComparisonChart();

            // Set initial connection status
            UpdateConnectionStatus();

            // Load initial trip history data
            _ = LoadTripHistoryAsync();
        }

        #region Properties

        /// <summary>
        /// Gets the current performance metrics for the active session.
        /// </summary>
        public PerformanceMetrics CurrentMetrics
        {
            get => _currentMetrics;
            private set => SetProperty(ref _currentMetrics, value);
        }

        /// <summary>
        /// Gets the current connection status message.
        /// </summary>
        public string ConnectionStatus
        {
            get => _connectionStatus;
            private set => SetProperty(ref _connectionStatus, value);
        }

        /// <summary>
        /// Gets the current vehicle speed in miles per hour.
        /// </summary>
        public float CurrentSpeed
        {
            get => _currentSpeed;
            private set => SetProperty(ref _currentSpeed, value);
        }

        private float _currentDamage;
        /// <summary>
        /// Gets the current vehicle damage percentage (0-100%).
        /// </summary>
        public float CurrentDamage
        {
            get => _currentDamage;
            private set => SetProperty(ref _currentDamage, value);
        }

        /// <summary>
        /// Gets the score breakdown showing component contributions.
        /// </summary>
        public ScoreBreakdown ScoreBreakdown
        {
            get => _scoreBreakdown;
            private set => SetProperty(ref _scoreBreakdown, value);
        }

        /// <summary>
        /// Gets or sets whether the breakdown panel is expanded.
        /// </summary>
        public bool IsBreakdownExpanded
        {
            get => _isBreakdownExpanded;
            set => SetProperty(ref _isBreakdownExpanded, value);
        }

        /// <summary>
        /// Gets the collection of recent performance notifications.
        /// Limited to the 5 most recent notifications.
        /// </summary>
        public ObservableCollection<PerformanceNotification> RecentNotifications
        {
            get => _recentNotifications;
            private set => SetProperty(ref _recentNotifications, value);
        }

        /// <summary>
        /// Gets the smoothness score sparkline chart series.
        /// </summary>
        public ISeries[] SmoothnessChartSeries
        {
            get => _smoothnessChartSeries;
            private set => SetProperty(ref _smoothnessChartSeries, value);
        }

        /// <summary>
        /// Gets the speed compliance sparkline chart series.
        /// </summary>
        public ISeries[] SpeedComplianceChartSeries
        {
            get => _speedComplianceChartSeries;
            private set => SetProperty(ref _speedComplianceChartSeries, value);
        }

        /// <summary>
        /// Gets the safety score sparkline chart series.
        /// </summary>
        public ISeries[] SafetyChartSeries
        {
            get => _safetyChartSeries;
            private set => SetProperty(ref _safetyChartSeries, value);
        }

        /// <summary>
        /// Gets the overall score sparkline chart series.
        /// </summary>
        public ISeries[] OverallChartSeries
        {
            get => _overallChartSeries;
            private set => SetProperty(ref _overallChartSeries, value);
        }

        /// <summary>
        /// Gets the tooltip information for the smoothness metric.
        /// </summary>
        public MetricTooltipInfo SmoothnessTooltip => _performanceCalculator.GetSmoothnessTooltip();

        /// <summary>
        /// Gets the tooltip information for the speed compliance metric.
        /// </summary>
        public MetricTooltipInfo SpeedComplianceTooltip => _performanceCalculator.GetSpeedComplianceTooltip();

        /// <summary>
        /// Gets the tooltip information for the safety metric.
        /// </summary>
        public MetricTooltipInfo SafetyTooltip => _performanceCalculator.GetSafetyTooltip();

        /// <summary>
        /// Gets the tooltip information for the damage-free metric.
        /// Uses the safety tooltip as damage-free is a safety-related metric.
        /// </summary>
        public MetricTooltipInfo DamageFreeTooltip => _performanceCalculator.GetSafetyTooltip();

        /// <summary>
        /// Gets the tooltip information for the overall score metric.
        /// </summary>
        public MetricTooltipInfo OverallTooltip => _performanceCalculator.GetOverallTooltip();

        /// <summary>
        /// Gets the collection of recent trips from the database.
        /// </summary>
        public ObservableCollection<Trip> RecentTrips
        {
            get => _recentTrips;
            private set => SetProperty(ref _recentTrips, value);
        }

        /// <summary>
        /// Gets the aggregate statistics calculated from all trips.
        /// </summary>
        public TripStatistics Statistics
        {
            get => _statistics;
            private set => SetProperty(ref _statistics, value);
        }

        /// <summary>
        /// Gets or sets the start date for filtering trips.
        /// </summary>
        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set => SetProperty(ref _filterStartDate, value);
        }

        /// <summary>
        /// Gets or sets the end date for filtering trips.
        /// </summary>
        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set => SetProperty(ref _filterEndDate, value);
        }

        /// <summary>
        /// Gets or sets whether a loading operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Gets or sets the loading message to display during operations.
        /// </summary>
        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        /// <summary>
        /// Gets the command to filter trips by date range.
        /// </summary>
        public ICommand FilterByDateCommand { get; }

        /// <summary>
        /// Gets the command to export trip data to CSV file.
        /// </summary>
        public ICommand ExportToCSVCommand { get; }

        /// <summary>
        /// Gets the performance trend chart series showing overall grades for the last 10 trips.
        /// </summary>
        public ISeries[] PerformanceTrendSeries
        {
            get => _performanceTrendSeries;
            private set => SetProperty(ref _performanceTrendSeries, value);
        }

        /// <summary>
        /// Gets the comparison bar chart series comparing current session to personal averages.
        /// </summary>
        public ISeries[] ComparisonChartSeries
        {
            get => _comparisonChartSeries;
            private set => SetProperty(ref _comparisonChartSeries, value);
        }

        /// <summary>
        /// Gets the X axes for the comparison chart.
        /// </summary>
        public LiveChartsCore.Kernel.Sketches.ICartesianAxis[] ComparisonXAxes
        {
            get => _comparisonXAxes;
            private set => SetProperty(ref _comparisonXAxes, value);
        }

        /// <summary>
        /// Gets the Y axes for the comparison chart.
        /// </summary>
        public LiveChartsCore.Kernel.Sketches.ICartesianAxis[] ComparisonYAxes
        {
            get => _comparisonYAxes;
            private set => SetProperty(ref _comparisonYAxes, value);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles telemetry data updates and refreshes performance metrics.
        /// Updates are dispatched to the UI thread to ensure thread safety.
        /// </summary>
        private void OnTelemetryDataUpdated(object? sender, TelemetryData data)
        {
            if (data == null) return;

            // Update properties on UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                // Update current speed
                // Note: If speed seems too high, telemetry might be sending km/h instead of m/s
                // Try 0.621371f (km/h to MPH) if 2.23694f (m/s to MPH) seems wrong
                CurrentSpeed = data.Speed * 0.621371f; // Assuming km/h from telemetry

                // Update current damage percentage from telemetry
                CurrentDamage = data.DamagePercent;

                // Get updated metrics from calculator (returns a new copy each time)
                CurrentMetrics = _performanceCalculator.GetCurrentMetrics();
                ScoreBreakdown = _performanceCalculator.GetScoreBreakdown();

                // Notify tooltip property changes
                OnPropertyChanged(nameof(SmoothnessTooltip));
                OnPropertyChanged(nameof(SpeedComplianceTooltip));
                OnPropertyChanged(nameof(SafetyTooltip));
                OnPropertyChanged(nameof(DamageFreeTooltip));
                OnPropertyChanged(nameof(OverallTooltip));

                // Update sparkline charts
                UpdateSparklineCharts();

                // Update comparison chart
                UpdateComparisonChart();
            });
        }

        /// <summary>
        /// Handles connection status changes from the telemetry client.
        /// </summary>
        private void OnConnectionStatusChanged(object? sender, string status)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ConnectionStatus = status ?? "Disconnected";
            });
        }

        /// <summary>
        /// Updates the connection status based on the current telemetry client state.
        /// </summary>
        private void UpdateConnectionStatus()
        {
            ConnectionStatus = _telemetryClient.IsConnected ? "Connected" : "Disconnected";
        }

        /// <summary>
        /// Handles performance notification events from the calculator.
        /// Adds notifications to the persistent event log and limits to 100 most recent events.
        /// </summary>
        private void OnNotificationRaised(object? sender, PerformanceNotification notification)
        {
            if (notification == null) return;

            // Update on UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                // Add to the beginning of the collection (newest first)
                RecentNotifications.Insert(0, notification);

                // Limit to 100 most recent notifications to prevent memory issues
                while (RecentNotifications.Count > 100)
                {
                    RecentNotifications.RemoveAt(RecentNotifications.Count - 1);
                }
            });
        }

        /// <summary>
        /// Cleanup timer tick handler - no longer removes old notifications.
        /// Kept for potential future use or can be removed.
        /// </summary>
        private void CleanupOldNotifications(object? sender, EventArgs e)
        {
            // No longer auto-removing notifications - they persist for the session
            // This method is kept for potential future cleanup logic
        }

        #endregion

        #region Sparkline Chart Methods

        /// <summary>
        /// Initializes the sparkline chart series with default styling.
        /// </summary>
        private void InitializeSparklineCharts()
        {
            // Create smoothness chart (green)
            _smoothnessChartSeries = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = new List<float>(),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 2 },
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            };

            // Create speed compliance chart (blue)
            _speedComplianceChartSeries = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = new List<float>(),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            };

            // Create safety chart (orange)
            _safetyChartSeries = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = new List<float>(),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            };

            // Create overall chart (purple)
            _overallChartSeries = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = new List<float>(),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 2 },
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            };
        }

        /// <summary>
        /// Updates the sparkline charts with the latest historical data.
        /// </summary>
        private void UpdateSparklineCharts()
        {
            if (CurrentMetrics == null) return;

            // Update smoothness chart
            if (_smoothnessChartSeries[0] is LineSeries<float> smoothnessSeries)
            {
                smoothnessSeries.Values = CurrentMetrics.SmoothnessHistory;
            }

            // Update speed compliance chart
            if (_speedComplianceChartSeries[0] is LineSeries<float> speedSeries)
            {
                speedSeries.Values = CurrentMetrics.SpeedComplianceHistory;
            }

            // Update safety chart
            if (_safetyChartSeries[0] is LineSeries<float> safetySeries)
            {
                safetySeries.Values = CurrentMetrics.SafetyHistory;
            }

            // Update overall chart (damage-free streak card)
            if (_overallChartSeries[0] is LineSeries<float> overallSeries)
            {
                overallSeries.Values = CurrentMetrics.OverallHistory;
            }
        }

        #endregion

        #region Performance Trend Chart Methods

        /// <summary>
        /// Initializes the performance trend chart series with default styling.
        /// </summary>
        private void InitializePerformanceTrendChart()
        {
            _performanceTrendSeries = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = new List<float>(),
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 3 },
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    LineSmoothness = 0.3,
                    Name = "Overall Grade"
                }
            };
        }

        /// <summary>
        /// Updates the performance trend chart with the last 10 trips' overall grades.
        /// Converts letter grades to percentage scores for visualization.
        /// </summary>
        private void UpdatePerformanceTrendChart()
        {
            try
            {
                // Take the last 10 trips (or fewer if less than 10 exist)
                var last10Trips = RecentTrips.Take(10).Reverse().ToList();

                // Convert overall grades to percentage scores
                var gradeScores = last10Trips.Select(trip => ScoreHelper.ConvertGradeToScore(trip.OverallGrade)).ToList();

                // Update the chart series
                if (_performanceTrendSeries[0] is LineSeries<float> series)
                {
                    series.Values = gradeScores;
                }

                // Notify property changed to refresh the chart
                OnPropertyChanged(nameof(PerformanceTrendSeries));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to update performance trend chart");
            }
        }



        #endregion

        #region Comparison Chart Methods

        /// <summary>
        /// Initializes the comparison bar chart series with default styling.
        /// Creates two series: one for current session and one for personal averages.
        /// </summary>
        private void InitializeComparisonChart()
        {
            _comparisonChartSeries = new ISeries[]
            {
                new ColumnSeries<float>
                {
                    Values = new List<float> { 0, 0, 0 },
                    Name = "Current Session",
                    Fill = new SolidColorPaint(SKColors.Purple),
                    Stroke = null,
                    MaxBarWidth = 60
                },
                new ColumnSeries<float>
                {
                    Values = new List<float> { 0, 0, 0 },
                    Name = "Personal Average",
                    Fill = new SolidColorPaint(SKColors.Gray),
                    Stroke = null,
                    MaxBarWidth = 60
                }
            };

            // Configure X axis with metric labels
            _comparisonXAxes = new LiveChartsCore.Kernel.Sketches.ICartesianAxis[]
            {
                new Axis
                {
                    Labels = new[] { "Smoothness", "Speed\nCompliance", "Safety" },
                    LabelsPaint = new SolidColorPaint(new SKColor(127, 140, 141))
                }
            };

            // Configure Y axis with 0-100 range
            _comparisonYAxes = new LiveChartsCore.Kernel.Sketches.ICartesianAxis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    LabelsPaint = new SolidColorPaint(new SKColor(127, 140, 141))
                }
            };
        }

        /// <summary>
        /// Updates the comparison bar chart with current session metrics compared to personal averages.
        /// Colors bars green if above average, red if below average.
        /// </summary>
        private void UpdateComparisonChart()
        {
            try
            {
                if (CurrentMetrics == null || Statistics == null)
                {
                    return;
                }

                // Get current session values
                var currentSmoothness = CurrentMetrics.SmoothnessScore;
                var currentSpeedCompliance = CurrentMetrics.SpeedCompliancePercent;
                var currentSafety = CurrentMetrics.SafetyScore;

                // Get personal averages
                var avgSmoothness = Statistics.AverageSmoothnessScore;
                var avgSpeedCompliance = Statistics.AverageSpeedCompliancePercent;
                var avgSafety = Statistics.AverageSafetyScore;

                // Update current session series
                if (_comparisonChartSeries[0] is ColumnSeries<float> currentSeries)
                {
                    currentSeries.Values = new List<float>
                    {
                        currentSmoothness,
                        currentSpeedCompliance,
                        currentSafety
                    };

                    // Color based on performance vs average
                    var avgOverall = (avgSmoothness + avgSpeedCompliance + avgSafety) / 3.0f;
                    var currentOverall = (currentSmoothness + currentSpeedCompliance + currentSafety) / 3.0f;

                    if (currentOverall >= avgOverall)
                    {
                        // Above average - green
                        currentSeries.Fill = new SolidColorPaint(new SKColor(39, 174, 96)); // #27AE60
                    }
                    else
                    {
                        // Below average - red
                        currentSeries.Fill = new SolidColorPaint(new SKColor(231, 76, 60)); // #E74C3C
                    }
                }

                // Update personal average series
                if (_comparisonChartSeries[1] is ColumnSeries<float> avgSeries)
                {
                    avgSeries.Values = new List<float>
                    {
                        avgSmoothness,
                        avgSpeedCompliance,
                        avgSafety
                    };
                }

                // Notify property changed to refresh the chart
                OnPropertyChanged(nameof(ComparisonChartSeries));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to update comparison chart");
            }
        }

        #endregion

        #region Trip History Methods

        /// <summary>
        /// Loads recent trips and statistics from the database.
        /// Called on initialization and after date filter changes.
        /// </summary>
        private async Task LoadTripHistoryAsync()
        {
            try
            {
                // Show loading indicator
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsLoading = true;
                    LoadingMessage = "Loading trip history...";
                });

                List<Trip> trips;

                // Load trips based on date filter
                if (FilterStartDate.HasValue && FilterEndDate.HasValue)
                {
                    trips = await _tripRepository.GetTripsByDateRangeAsync(
                        FilterStartDate.Value,
                        FilterEndDate.Value);
                }
                else
                {
                    trips = await _tripRepository.GetRecentTripsAsync(20);
                }

                // Load statistics
                var statistics = await _tripRepository.GetStatisticsAsync();

                // Update UI on dispatcher thread
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    RecentTrips.Clear();
                    foreach (var trip in trips)
                    {
                        RecentTrips.Add(trip);
                    }

                    Statistics = statistics;

                    // Update the performance trend chart with the new trip data
                    UpdatePerformanceTrendChart();

                    // Update the comparison chart with the new statistics
                    UpdateComparisonChart();
                });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to load trip history");
            }
            finally
            {
                // Hide loading indicator
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsLoading = false;
                });
            }
        }

        /// <summary>
        /// Filters trips by the selected date range.
        /// </summary>
        private async Task FilterByDateAsync()
        {
            await LoadTripHistoryAsync();
        }

        /// <summary>
        /// Checks if a new trip has been completed and updates the trip list.
        /// Called periodically by the trip check timer.
        /// </summary>
        private async void CheckForNewTrips(object? sender, EventArgs e)
        {
            try
            {
                // Check if we need to reload trips (simple approach: reload if trip count changed)
                var currentStats = await _tripRepository.GetStatisticsAsync();

                if (currentStats.TotalTrips != Statistics.TotalTrips)
                {
                    // New trip detected, reload the list
                    await LoadTripHistoryAsync();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to check for new trips");
            }
        }

        /// <summary>
        /// Exports all trip data to a CSV file.
        /// Prompts the user for a save location and writes all trip records with proper formatting.
        /// </summary>
        private async Task ExportToCSVAsync()
        {
            try
            {
                // Prompt user for save location using SaveFileDialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv",
                    FileName = $"ATS_Performance_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    Title = "Export Trip Data to CSV"
                };

                var result = saveFileDialog.ShowDialog();
                if (result != true)
                {
                    // User cancelled
                    return;
                }

                // Show loading indicator
                IsLoading = true;
                LoadingMessage = "Exporting trip data...";

                // Retrieve all trips from database
                var allTrips = await _tripRepository.GetAllTripsAsync();

                // Generate CSV content
                var csvBuilder = new System.Text.StringBuilder();

                // Add header row
                csvBuilder.AppendLine("Id,StartTime,EndTime,Duration,Distance,Smoothness,MPG,SpeedCompliance,Grade,AvgSpeed,FuelConsumed");

                // Add data rows
                foreach (var trip in allTrips)
                {
                    // Format timestamps as ISO 8601
                    var startTime = trip.StartTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
                    var endTime = trip.EndTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

                    csvBuilder.AppendLine(
                        $"{trip.Id}," +
                        $"{startTime}," +
                        $"{endTime}," +
                        $"{trip.DurationMinutes:F2}," +
                        $"{trip.DistanceMiles:F2}," +
                        $"{trip.SmoothnessScore:F2}," +
                        $"{trip.FuelEfficiencyMPG:F2}," +
                        $"{trip.SpeedCompliancePercent:F2}," +
                        $"{trip.OverallGrade}," +
                        $"{trip.AverageSpeed:F2}," +
                        $"{trip.FuelConsumed:F2}");
                }

                // Write to file
                await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, csvBuilder.ToString());

                // Show success message
                MessageBox.Show(
                    $"Successfully exported {allTrips.Count} trips to:\n{saveFileDialog.FileName}",
                    "Export Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Serilog.Log.Information("Exported {Count} trips to CSV file: {FilePath}", allTrips.Count, saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to export trips to CSV");

                // Show error message
                MessageBox.Show(
                    $"Failed to export trip data:\n{ex.Message}",
                    "Export Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Hide loading indicator
                IsLoading = false;
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Stop and dispose timers
            _notificationCleanupTimer?.Stop();
            _tripCheckTimer?.Stop();

            // Unsubscribe from events
            _telemetryClient.DataUpdated -= OnTelemetryDataUpdated;
            _telemetryClient.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _performanceCalculator.NotificationRaised -= OnNotificationRaised;

            _disposed = true;
        }

        #endregion
    }
}
