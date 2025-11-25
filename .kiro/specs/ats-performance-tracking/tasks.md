# Implementation Plan

This implementation plan breaks down the ATS Performance Tracking System into discrete, actionable coding tasks. Each task builds incrementally on previous work, focusing on Phase 1 (Real-Time Scoring) first, followed by Phase 2 (Persistence) and Phase 3 (Analytics).

## Phase 1: Real-Time Performance Scoring (MVP)

- [x] 1. Extend telemetry data model with performance tracking fields





  - Modify `TelemetryData.cs` to add properties: `FuelAmount`, `FuelCapacity`, `DamagePercent`, `Odometer`, `SpeedLimit`
  - Update `HttpTelemetryClient.ParseTelemetryJson()` to extract these fields from Funbit API JSON response
  - Handle missing fields gracefully with default values
  - _Requirements: 1.1, 2.1, 3.1, 4.2_

- [x] 2. Create core performance tracking models





  - Create `PerformanceMetrics.cs` model class with all score properties (smoothness, fuel efficiency, speed compliance, damage-free streak, overall grade)
  - Create `TrendIndicator.cs` enum with Up, Down, Stable values
  - Add XML documentation comments to all public properties
  - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1_

- [x] 3. Implement PerformanceCalculator service





  - Create `IPerformanceCalculator.cs` interface with methods: `GetCurrentMetrics()`, `UpdateFromTelemetry()`, `ResetSession()`
  - Create `PerformanceCalculator.cs` implementation class
  - Implement internal state tracking (previous speed, timestamps, session start values)
  - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1_

- [x] 3.1 Implement smoothness score calculation algorithm


  - Calculate acceleration change between telemetry updates
  - Apply penalty (-2 points) for acceleration > 5 MPH/sec
  - Apply penalty (-5 points) for deceleration > 10 MPH/sec
  - Clamp score to 0-100 range
  - Update score every telemetry update
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 3.2 Implement fuel efficiency calculation algorithm

  - Track distance traveled from odometer or position changes
  - Track fuel consumed from fuel level changes
  - Calculate MPG (miles per gallon)
  - Calculate efficiency score as percentage of 6 MPG baseline
  - Calculate percentage difference from baseline
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [x] 3.3 Implement speed compliance calculation algorithm

  - Determine current speed limit (65 MPH highway, 55 MPH default)
  - Track time spent at or below speed limit
  - Calculate compliance percentage (time compliant / total time)
  - Convert percentage to letter grade
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 3.4 Implement damage-free streak tracking

  - Initialize streak to zero at session start
  - Monitor damage percentage from telemetry
  - Reset streak to zero when damage increases
  - Increment streak by elapsed time when damage is constant
  - Format streak as HH:MM string
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [x] 3.5 Implement overall grade calculation

  - Average smoothness, fuel efficiency, and speed compliance scores
  - Convert average score to letter grade (A+ to F scale)
  - Calculate trend indicator by comparing to session average
  - Track session statistics (duration, distance, average speed)
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

- [x] 4. Create PerformanceViewModel





  - Create `PerformanceViewModel.cs` implementing `INotifyPropertyChanged`
  - Add observable properties for `CurrentMetrics`, `ConnectionStatus`, `CurrentSpeed`
  - Inject `IPerformanceCalculator` and `ITelemetryClient` via constructor
  - Subscribe to telemetry `DataUpdated` event
  - Update ViewModel properties on UI thread using `Dispatcher.Invoke()`
  - _Requirements: 1.5, 2.5, 3.5, 4.4, 5.6, 9.6_

- [x] 5. Create PerformanceWindow UI layout





  - Create `PerformanceWindow.xaml` and code-behind
  - Implement menu bar with File menu (placeholder for export)
  - Create 4-column UniformGrid for metric cards
  - Add overall grade display section with trend indicator
  - Add status bar showing connection status and current speed
  - Set window size and title
  - _Requirements: 9.1, 9.6_

- [x] 5.1 Implement metric card UI components


  - Create reusable card style with white background and subtle shadow
  - Build smoothness score card with percentage and grade display
  - Build fuel efficiency card with MPG and percentage difference
  - Build speed compliance card with percentage and grade
  - Build damage-free streak card with time display (HH:MM format)
  - Bind all cards to ViewModel properties
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [x] 5.2 Implement grade-based color styling


  - Create `GradeToColorConverter.cs` value converter
  - Map A+/A grades to green (#27AE60)
  - Map B+/B grades to blue (#3498DB)
  - Map C grades to yellow (#F39C12)
  - Map D/F grades to red (#E74C3C)
  - Apply converter to grade displays in XAML
  - _Requirements: 9.2, 9.3, 9.4, 9.5_

- [x] 5.3 Implement trend indicator display


  - Create `TrendToSymbolConverter.cs` to convert enum to arrow symbols (↑↓→)
  - Display trend symbol next to overall grade
  - Show percentage change from session average
  - Apply color based on trend direction (green up, red down, gray stable)
  - _Requirements: 5.6_

- [x] 6. Register services in dependency injection container





  - Modify `App.xaml.cs` `ConfigureServices()` method
  - Register `IPerformanceCalculator` as singleton with `PerformanceCalculator` implementation
  - Register `PerformanceViewModel` as singleton
  - Register `PerformanceWindow` as transient
  - Wire up PerformanceCalculator to receive telemetry updates
  - _Requirements: 1.5, 2.5, 3.5, 4.4, 5.6_

- [x] 7. Add performance window launch mechanism







  - Add menu item or button to MainWindow to open PerformanceWindow
  - Implement command or click handler to show PerformanceWindow
  - Ensure only one instance of PerformanceWindow can be open
  - Position PerformanceWindow next to MainWindow
  - _Requirements: 9.1, 9.6_

- [ ]* 8. Create unit tests for PerformanceCalculator
  - Test smoothness score with various acceleration patterns
  - Test fuel efficiency calculation with known values
  - Test speed compliance with different scenarios
  - Test damage-free streak reset logic
  - Test overall grade conversion for all score ranges
  - Test trend calculation logic
  - _Requirements: 1.1-1.5, 2.1-2.5, 3.1-3.5, 4.1-4.4, 5.1-5.6_

## Phase 1.5: Enhanced Safety Metrics & UI Improvements

- [x] 9. Implement Safety Score calculation





  - Add SafetyScore property to PerformanceMetrics model
  - Track blinker usage during turns (detect heading changes without blinkers active)
  - Penalize driving with parking brake engaged
  - Penalize inappropriate high beam usage
  - Reward proper engine brake/retarder usage
  - Penalize over-revving engine (RPM > 90% of max)
  - Penalize excessive brake temperature (> 200°C indicates poor braking technique)
  - Initialize safety score at 100% and apply penalties/rewards over time
  - _Requirements: Safety and compliance tracking_

- [x] 9.1 Add safety score to overall grade calculation





  - Update overall grade formula to include safety score
  - Weight: 25% smoothness, 25% speed compliance, 25% safety, 25% damage-free time
  - Ensure safety score is displayed in UI
  - _Requirements: Comprehensive performance rating_

  - [x] 9.2 Display warnings to the user when they do something that affects their score






- [x] 10. Create performance breakdown transparency panel





  - Add expandable section to PerformanceWindow showing score breakdown
  - Display each metric component with its current value and contribution
  - Show real-time penalties/rewards as they occur
  - Use color coding (green for positive, red for negative, gray for neutral)
  - Add icons for each metric type (🚦 speed, 🎯 smoothness, 🛡️ safety, etc.)
  - _Requirements: User transparency in rating calculation_


- [x] 10.1 Implement live penalty/reward notifications





  - Create notification panel showing recent events affecting score
  - Display messages like "Sharp turn without blinker -2 pts" or "Smooth braking +0.5 pts"
  - Auto-fade notifications after 3 seconds
  - Limit to 5 most recent notifications visible
  - _Requirements: Real-time feedback transparency_

- [x] 11. Beautify PerformanceWindow UI




  - Redesign metric cards with modern styling (gradients, better shadows)
  - Add animated progress bars showing score values
  - Implement smooth color transitions when scores change
  - Add icons to each metric card
  - Improve typography (better fonts, sizing, hierarchy)
  - Add subtle animations on score updates
  - Implement dark/light theme support
  - _Requirements: Professional, appealing UI design_

- [x] 11.1 Add visual score history mini-charts

  - Add small sparkline charts to each metric card showing last 60 seconds
  - Use LiveCharts or similar for lightweight visualization
  - Show trend at a glance without opening full analytics
  - _Requirements: Quick visual feedback_

- [x] 12. Implement detailed metrics tooltip system





  - Add hover tooltips explaining how each metric is calculated
  - Show current penalties/bonuses affecting the score
  - Display tips for improving each metric
  - Use rich tooltips with formatting and icons
  - _Requirements: Educational transparency_

## Phase 2: Trip History & Persistence


- [x] 9. Set up SQLite database infrastructure




  - Add `Microsoft.Data.Sqlite` NuGet package to ATSLiveMap.Core project
  - Create `Trip.cs` model class with all trip properties
  - Create `TripStatistics.cs` model class for aggregate statistics
  - Create database initialization script with Trips table schema
  - Add database path configuration to appsettings.json
  - _Requirements: 6.3, 6.4, 6.5_

- [x] 10. Implement TripRepository data access layer





  - Create `ITripRepository.cs` interface with async CRUD methods
  - Create `TripRepository.cs` implementation using Microsoft.Data.Sqlite
  - Implement `SaveTripAsync()` to insert trip records
  - Implement `GetRecentTripsAsync()` to retrieve last N trips
  - Implement `GetTripsByDateRangeAsync()` for date filtering
  - Implement `GetStatisticsAsync()` to calculate aggregate stats
  - Add proper connection disposal and error handling
  - _Requirements: 6.3, 6.4, 6.5, 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ]* 10.1 Create unit tests for TripRepository
  - Test CRUD operations with in-memory SQLite database
  - Test date range filtering accuracy
  - Test statistics calculation correctness
  - Test error handling for database failures
  - _Requirements: 6.3, 6.4, 6.5_

- [x] 11. Implement TripDetectionService





  - Create `ITripDetectionService.cs` interface with trip lifecycle methods
  - Create `TripDetectionService.cs` implementation
  - Implement trip start detection (speed > 0 for first time)
  - Implement trip end detection (speed = 0 for > 5 minutes)
  - Track trip duration, distance, and fuel consumption during trip
  - Capture performance metrics snapshot when trip ends
  - Integrate with TripRepository to save completed trips
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [ ]* 11.1 Create unit tests for TripDetectionService
  - Test trip start detection logic
  - Test trip end detection with 5-minute timeout
  - Test distance and fuel accumulation
  - Test trip end on application close scenario
  - _Requirements: 6.1, 6.2_

- [x] 12. Extend PerformanceViewModel for trip history




  - Add `ObservableCollection<Trip>` property for recent trips
  - Add `TripStatistics` property for aggregate statistics
  - Add date range filter properties and command
  - Inject `ITripRepository` via constructor
  - Load recent trips on ViewModel initialization
  - Update trips list when new trip is saved
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 13. Add trip history UI to PerformanceWindow





  - Add ListView control to display recent trips
  - Create trip item template showing trip number, timestamp, duration, distance, MPG, grade
  - Add date range filter controls (DatePicker for start/end dates)
  - Display summary statistics (total trips, average grade, best/worst trips)
  - Bind ListView to ViewModel's RecentTrips collection
  - Style trip items with icons and color-coded grades
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_
-

- [x] 14. Implement trip save on application close




  - Modify `App.xaml.cs` `OnExit()` method
  - Call `TripDetectionService.EndCurrentTripAsync()` before shutdown
  - Ensure trip is saved even if 5-minute timeout hasn't elapsed
  - Add error handling for database save failures
  - _Requirements: 6.2_

- [x] 15. Register Phase 2 services in DI container




  - Register `ITripRepository` as singleton
  - Register `ITripDetectionService` as singleton
  - Wire up TripDetectionService to receive telemetry updates
  - Wire up TripDetectionService to PerformanceCalculator for metrics
  - _Requirements: 6.1, 6.2, 6.3_

## Phase 3: Visual Analytics & Polish


- [x] 16. Add charting library dependency



  - Add `LiveCharts2.WPF` NuGet package to ATSLiveMap.UI project
  - Add LiveCharts namespace to PerformanceWindow.xaml
  - Configure chart default styles in App.xaml resources
  - _Requirements: 8.1, 8.2, 8.3, 8.4_
-

- [x] 17. Implement performance trend line chart




  - Add `ObservableCollection<ChartDataPoint>` property to PerformanceViewModel
  - Load last 10 trips' overall grades from database
  - Create LiveCharts LineSeries configuration
  - Add CartesianChart control to PerformanceWindow
  - Bind chart to ViewModel's performance history data
  - Configure X-axis (trip number) and Y-axis (grade percentage)
  - Style chart with colors matching grade scheme
  - _Requirements: 8.1, 8.4_


- [x] 18. Implement comparison bar chart




  - Calculate personal averages for each metric from trip history
  - Create bar chart data comparing current session to averages
  - Add ColumnSeries for each metric (smoothness, fuel efficiency, speed compliance)
  - Display percentage difference indicators
  - Color bars based on performance (green if above average, red if below)
  - _Requirements: 8.2, 8.3_

- [x] 19. Implement CSV export functionality





  - Create `ExportToCSVCommand` in PerformanceViewModel
  - Use `SaveFileDialog` to prompt user for save location
  - Generate CSV with headers: Id, StartTime, EndTime, Duration, Distance, Smoothness, MPG, SpeedCompliance, Grade, AvgSpeed, FuelConsumed
  - Format timestamps as ISO 8601
  - Write all trip records from database to CSV file
  - Show success/error message after export
  - _Requirements: 10.1, 10.2, 10.3, 10.4_

- [x] 20. Polish UI styling and animations





  - Add fade-in animations for metric cards
  - Add smooth transitions when grades change colors
  - Implement hover effects on trip list items
  - Add loading indicators for database operations
  - Ensure responsive layout for different window sizes
  - Add tooltips with detailed explanations for each metric
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

- [ ]* 21. Create integration tests for end-to-end flows
  - Test complete trip lifecycle (start, drive, stop, save)
  - Test metrics calculation with realistic telemetry data stream
  - Test UI updates in response to telemetry changes
  - Test chart rendering with trip history data
  - Test CSV export with sample data
  - _Requirements: 6.1-6.5, 7.1-7.5, 8.1-8.4, 10.1-10.4_

## Notes

- Tasks marked with `*` are optional testing tasks that can be skipped to focus on core functionality
- Each task references specific requirements from the requirements document
- Tasks are ordered to build incrementally - complete earlier tasks before later ones
- Phase 1 must be fully functional before starting Phase 2
- Phase 2 must be complete before starting Phase 3
