# Design Document

## Overview

The ATS Performance Tracking System extends the existing ATSLiveMap application to provide real-time driving performance analysis, historical trip tracking, and visual analytics. The system integrates seamlessly with the existing telemetry infrastructure (HttpTelemetryClient) and follows the established MVVM architecture pattern.

The design focuses on three incremental phases:
- **Phase 1**: Real-time performance scoring with live UI display
- **Phase 2**: Trip persistence using SQLite with automatic trip detection
- **Phase 3**: Visual analytics with charts and data export capabilities

This document covers the complete architecture for all three phases, with implementation prioritizing Phase 1 functionality first.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer (WPF)                  │
│  ┌────────────────────┐      ┌─────────────────────────┐   │
│  │  MainWindow        │      │ PerformanceWindow       │   │
│  │  (Existing)        │      │ (New)                   │   │
│  └────────────────────┘      └─────────────────────────┘   │
│           │                            │                     │
│           ▼                            ▼                     │
│  ┌────────────────────┐      ┌─────────────────────────┐   │
│  │  MainViewModel     │      │ PerformanceViewModel    │   │
│  │  (Existing)        │      │ (New)                   │   │
│  └────────────────────┘      └─────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                      Business Logic Layer                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  PerformanceCalculator (New)                         │  │
│  │  - CalculateSmoothnessScore()                        │  │
│  │  - CalculateFuelEfficiency()                         │  │
│  │  - CalculateSpeedCompliance()                        │  │
│  │  - CalculateOverallGrade()                           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  TripDetectionService (New - Phase 2)                │  │
│  │  - DetectTripStart()                                 │  │
│  │  - DetectTripEnd()                                   │  │
│  │  - SaveCurrentTrip()                                 │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Access Layer                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  TripRepository (New - Phase 2)                      │  │
│  │  - SaveTrip()                                        │  │
│  │  - GetRecentTrips()                                  │  │
│  │  - GetTripsByDateRange()                             │  │
│  │  - GetStatistics()                                   │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SQLite Database (trips.db)                          │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                               ▲
                               │
┌─────────────────────────────────────────────────────────────┐
│                    Telemetry Layer (Existing)                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  HttpTelemetryClient                                 │  │
│  │  - Polls localhost:25555 every 100ms                 │  │
│  │  - Provides: Speed, Position, Heading, Fuel, etc.   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Integration with Existing Architecture

The Performance Tracking System integrates with existing components:

1. **HttpTelemetryClient**: Subscribe to `DataUpdated` event to receive real-time telemetry
2. **Dependency Injection**: Register new services in `App.xaml.cs` using existing DI container
3. **MVVM Pattern**: Follow established pattern with ViewModels and Views
4. **WPF UI**: Create new window or integrate into existing MainWindow

## Components and Interfaces

### Core Models

#### PerformanceMetrics
```csharp
public class PerformanceMetrics
{
    public float SmoothnessScore { get; set; }           // 0-100%
    public float FuelEfficiencyMPG { get; set; }         // Current MPG
    public float FuelEfficiencyScore { get; set; }       // Percentage vs baseline
    public float SpeedCompliancePercent { get; set; }    // 0-100%
    public string SpeedComplianceGrade { get; set; }     // Letter grade
    public TimeSpan DamageFreeStreak { get; set; }       // Time since last damage
    public string OverallGrade { get; set; }             // A+ to F
    public float OverallScore { get; set; }              // 0-100%
    public TrendIndicator Trend { get; set; }            // Up, Down, Stable
    
    // Session statistics
    public float SessionDurationMinutes { get; set; }
    public float SessionDistanceMiles { get; set; }
    public float SessionAverageSpeed { get; set; }
    public float SessionFuelConsumed { get; set; }
}
```

#### Trip (Phase 2)
```csharp
public class Trip
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float DurationMinutes { get; set; }
    public float DistanceMiles { get; set; }
    public float SmoothnessScore { get; set; }
    public float FuelEfficiencyMPG { get; set; }
    public float SpeedCompliancePercent { get; set; }
    public string OverallGrade { get; set; }
    public float AverageSpeed { get; set; }
    public float FuelConsumed { get; set; }
}
```

#### TrendIndicator (Enum)
```csharp
public enum TrendIndicator
{
    Up,      // ↑ Performance improving
    Down,    // ↓ Performance declining
    Stable   // → Performance stable
}
```

### Services

#### IPerformanceCalculator
```csharp
public interface IPerformanceCalculator
{
    PerformanceMetrics GetCurrentMetrics();
    void UpdateFromTelemetry(TelemetryData data);
    void ResetSession();
}
```

**Implementation: PerformanceCalculator**

Responsibilities:
- Subscribe to telemetry updates
- Maintain running calculations for all metrics
- Track historical data points for trend analysis
- Calculate scores based on defined algorithms

Key algorithms:

**Smoothness Score Algorithm:**
```csharp
// Start at 100%, deduct points for harsh inputs
smoothnessScore = 100.0f;
float speedChange = Math.Abs(currentSpeed - previousSpeed);
float deltaTime = (currentTime - previousTime).TotalSeconds;

if (deltaTime > 0)
{
    float acceleration = speedChange / deltaTime;
    
    // Convert m/s to MPH for threshold comparison
    float accelerationMPH = acceleration * 2.237f;
    
    // Penalize sudden acceleration (> 5 MPH/sec)
    if (accelerationMPH > 5.0f)
    {
        smoothnessScore -= 2.0f;
    }
    
    // Penalize harsh braking (> 10 MPH/sec deceleration)
    if (currentSpeed < previousSpeed && accelerationMPH > 10.0f)
    {
        smoothnessScore -= 5.0f;
    }
}

// Clamp to 0-100 range
smoothnessScore = Math.Max(0, Math.Min(100, smoothnessScore));
```

**Fuel Efficiency Algorithm:**
```csharp
// Calculate MPG
float distanceMiles = totalDistanceMeters * 0.000621371f;
float fuelGallons = totalFuelLiters * 0.264172f;
float mpg = fuelGallons > 0 ? distanceMiles / fuelGallons : 0;

// Calculate efficiency score (6 MPG baseline)
float efficiencyScore = (mpg / 6.0f) * 100.0f;
float percentDifference = ((mpg - 6.0f) / 6.0f) * 100.0f;
```

**Speed Compliance Algorithm:**
```csharp
// Determine speed limit (simplified approach)
float speedLimit = IsHighway() ? 65.0f : 55.0f;

// Track compliance over time
if (currentSpeedMPH <= speedLimit)
{
    timeCompliant += deltaTime;
}
totalTime += deltaTime;

// Calculate compliance percentage
float compliancePercent = (timeCompliant / totalTime) * 100.0f;
```

**Overall Grade Conversion:**
```csharp
string ConvertScoreToGrade(float score)
{
    return score switch
    {
        >= 95.0f => "A+",
        >= 90.0f => "A",
        >= 85.0f => "B+",
        >= 80.0f => "B",
        >= 75.0f => "C+",
        >= 70.0f => "C",
        >= 65.0f => "D+",
        >= 60.0f => "D",
        _ => "F"
    };
}
```

#### ITripRepository (Phase 2)
```csharp
public interface ITripRepository
{
    Task<int> SaveTripAsync(Trip trip);
    Task<List<Trip>> GetRecentTripsAsync(int count = 20);
    Task<List<Trip>> GetTripsByDateRangeAsync(DateTime start, DateTime end);
    Task<TripStatistics> GetStatisticsAsync();
}
```

**Implementation: TripRepository**

Uses Microsoft.Data.Sqlite for database operations.

Database schema:
```sql
CREATE TABLE IF NOT EXISTS Trips (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    DurationMinutes REAL NOT NULL,
    DistanceMiles REAL NOT NULL,
    SmoothnessScore REAL NOT NULL,
    FuelEfficiencyMPG REAL NOT NULL,
    SpeedCompliancePercent REAL NOT NULL,
    OverallGrade TEXT NOT NULL,
    AverageSpeed REAL NOT NULL,
    FuelConsumed REAL NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_trips_starttime ON Trips(StartTime DESC);
```

#### ITripDetectionService (Phase 2)
```csharp
public interface ITripDetectionService
{
    bool IsTripActive { get; }
    Trip? CurrentTrip { get; }
    
    void UpdateFromTelemetry(TelemetryData data);
    Task<Trip?> EndCurrentTripAsync();
}
```

**Implementation: TripDetectionService**

Trip detection logic:
- **Trip Start**: Vehicle speed > 0 MPH for first time
- **Trip End**: Vehicle speed = 0 MPH for > 5 minutes OR application closing
- Tracks trip start time, accumulates distance and fuel consumption
- Captures final performance metrics when trip ends

### ViewModels

#### PerformanceViewModel
```csharp
public class PerformanceViewModel : INotifyPropertyChanged
{
    // Current session metrics (observable properties)
    public PerformanceMetrics CurrentMetrics { get; set; }
    
    // Recent trips (Phase 2)
    public ObservableCollection<Trip> RecentTrips { get; set; }
    
    // Statistics (Phase 2)
    public TripStatistics Statistics { get; set; }
    
    // Chart data (Phase 3)
    public ObservableCollection<ChartDataPoint> PerformanceHistory { get; set; }
    
    // Commands
    public ICommand ExportToCSVCommand { get; set; }
    public ICommand FilterByDateCommand { get; set; }
    
    // Constructor injects dependencies
    public PerformanceViewModel(
        IPerformanceCalculator calculator,
        ITripRepository repository,
        ITelemetryClient telemetryClient)
    {
        // Subscribe to telemetry updates
        // Update UI properties when metrics change
    }
}
```

### Views

#### PerformanceWindow.xaml

Layout structure:

```xml
<Window>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Menu -->
            <RowDefinition Height="Auto"/> <!-- Current Session Cards -->
            <RowDefinition Height="*"/>    <!-- Charts (Phase 3) -->
            <RowDefinition Height="*"/>    <!-- Recent Trips (Phase 2) -->
            <RowDefinition Height="Auto"/> <!-- Status Bar -->
        </Grid.RowDefinitions>
        
        <!-- Menu Bar -->
        <Menu Grid.Row="0">
            <MenuItem Header="File">
                <MenuItem Header="Export to CSV" Command="{Binding ExportToCSVCommand}"/>
            </MenuItem>
        </Menu>
        
        <!-- Current Session Metrics -->
        <UniformGrid Grid.Row="1" Rows="1" Columns="4">
            <Border Style="{StaticResource MetricCardStyle}">
                <!-- Smoothness Score Card -->
            </Border>
            <Border Style="{StaticResource MetricCardStyle}">
                <!-- Fuel Efficiency Card -->
            </Border>
            <Border Style="{StaticResource MetricCardStyle}">
                <!-- Speed Compliance Card -->
            </Border>
            <Border Style="{StaticResource MetricCardStyle}">
                <!-- Damage-Free Streak Card -->
            </Border>
        </UniformGrid>
        
        <!-- Overall Grade Display -->
        <Border Grid.Row="1" Margin="10">
            <StackPanel>
                <TextBlock Text="{Binding CurrentMetrics.OverallGrade}"/>
                <TextBlock Text="{Binding CurrentMetrics.Trend}"/>
            </StackPanel>
        </Border>
        
        <!-- Performance History Chart (Phase 3) -->
        <lvc:CartesianChart Grid.Row="2" Series="{Binding PerformanceHistory}"/>
        
        <!-- Recent Trips List (Phase 2) -->
        <ListView Grid.Row="3" ItemsSource="{Binding RecentTrips}">
            <!-- Trip item template -->
        </ListView>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="4">
            <TextBlock Text="{Binding ConnectionStatus}"/>
            <TextBlock Text="{Binding CurrentSpeed}"/>
        </StatusBar>
    </Grid>
</Window>
```

## Data Models

### Extended TelemetryData Requirements

The existing `TelemetryData` class provides:
- ✅ Speed (m/s)
- ✅ Position (Vector3)
- ✅ Heading, Pitch, Roll
- ✅ Timestamp

**Additional data needed from Funbit API:**

Research into the Funbit telemetry server API shows these additional fields are available:
- `truck.fuel.value` - Current fuel amount
- `truck.fuel.capacity` - Tank capacity
- `truck.damage.total` - Total damage percentage
- `truck.odometer` - Total distance traveled
- `navigation.speedLimit` - Current speed limit (if available)

**Recommendation**: Extend `TelemetryData` class to include:
```csharp
public class TelemetryData
{
    // Existing properties...
    
    // New properties for performance tracking
    public float FuelAmount { get; set; }        // Current fuel in liters
    public float FuelCapacity { get; set; }      // Tank capacity in liters
    public float DamagePercent { get; set; }     // Total damage 0-100%
    public float Odometer { get; set; }          // Total km traveled
    public float SpeedLimit { get; set; }        // Current speed limit (0 if unknown)
}
```

Update `HttpTelemetryClient.ParseTelemetryJson()` to extract these fields.

### Performance Calculation State

The `PerformanceCalculator` maintains internal state:

```csharp
private class CalculationState
{
    // For smoothness calculation
    public float PreviousSpeed { get; set; }
    public DateTime PreviousTimestamp { get; set; }
    public List<float> RecentAccelerations { get; set; } = new();
    
    // For fuel efficiency
    public float SessionStartFuel { get; set; }
    public float SessionStartOdometer { get; set; }
    
    // For speed compliance
    public float TimeCompliant { get; set; }
    public float TotalTime { get; set; }
    
    // For damage tracking
    public float LastDamagePercent { get; set; }
    public DateTime DamageFreeStartTime { get; set; }
    
    // For session tracking
    public DateTime SessionStartTime { get; set; }
    public float SessionStartDistance { get; set; }
}
```

## Error Handling

### Telemetry Connection Loss

**Scenario**: HttpTelemetryClient loses connection to Funbit server

**Handling**:
1. PerformanceCalculator pauses metric updates
2. UI displays "Connection Lost" indicator
3. Current session data is preserved
4. Metrics resume updating when connection restored
5. No trip is saved if connection lost for < 5 minutes

### Database Errors (Phase 2)

**Scenario**: SQLite database file is locked or corrupted

**Handling**:
1. Log error with Serilog (existing logging infrastructure)
2. Display user-friendly error message
3. Continue operating in memory-only mode
4. Attempt to save trip data to backup JSON file
5. Provide option to export current session manually

### Missing Telemetry Fields

**Scenario**: Funbit API doesn't provide expected fields (fuel, damage, etc.)

**Handling**:
1. Use default/fallback values:
   - Fuel: Assume full tank, calculate consumption from speed/time estimates
   - Damage: Assume 0% if not available
   - Speed limit: Use default 65 MPH highway / 55 MPH other
2. Display warning icon on affected metrics
3. Log missing fields for debugging

### Invalid Metric Values

**Scenario**: Calculated metrics produce invalid results (NaN, negative, etc.)

**Handling**:
1. Clamp all scores to valid ranges (0-100%)
2. Display "N/A" for metrics that cannot be calculated
3. Log calculation errors
4. Continue calculating other metrics

## Testing Strategy

### Unit Tests

**PerformanceCalculator Tests**:
- Test smoothness score calculation with various acceleration patterns
- Test fuel efficiency calculation with known distance/fuel values
- Test speed compliance with different speed limit scenarios
- Test grade conversion for all score ranges
- Test trend calculation logic

**TripRepository Tests** (Phase 2):
- Test CRUD operations with in-memory SQLite database
- Test date range filtering
- Test statistics calculation
- Test concurrent access scenarios

**TripDetectionService Tests** (Phase 2):
- Test trip start detection
- Test trip end detection (5-minute timeout)
- Test trip end on application close
- Test distance accumulation

### Integration Tests

**End-to-End Performance Tracking**:
1. Mock telemetry data stream with realistic driving patterns
2. Verify metrics update in real-time
3. Verify UI reflects calculated values
4. Verify trends are calculated correctly

**Trip Persistence Flow** (Phase 2):
1. Simulate complete trip (start, drive, stop)
2. Verify trip is saved to database
3. Verify trip appears in recent trips list
4. Verify statistics are updated

### Manual Testing Scenarios

**Phase 1 Testing**:
- [ ] Launch app with ATS running, verify metrics display
- [ ] Drive smoothly, verify high smoothness score
- [ ] Brake hard, verify smoothness score decreases
- [ ] Drive at various speeds, verify fuel efficiency updates
- [ ] Exceed speed limit, verify compliance percentage drops
- [ ] Cause damage, verify damage-free streak resets
- [ ] Check overall grade matches individual scores

**Phase 2 Testing**:
- [ ] Complete a trip, stop for 5+ minutes, verify trip saved
- [ ] Close app during trip, verify trip saved on exit
- [ ] View recent trips list, verify correct data
- [ ] Filter trips by date range
- [ ] Verify statistics (total trips, average grade)

**Phase 3 Testing**:
- [ ] Complete 10+ trips, verify chart displays correctly
- [ ] Verify trend indicators match performance changes
- [ ] Export to CSV, verify file format and data accuracy
- [ ] Verify bar chart compares to personal averages

## Dependencies

### NuGet Packages

**Existing**:
- Microsoft.Extensions.DependencyInjection (8.0.0)
- System.Text.Json (10.0.0)
- Serilog (4.3.0)

**New for Phase 2**:
- Microsoft.Data.Sqlite (8.0.0) - SQLite database access

**New for Phase 3**:
- LiveCharts2.WPF (2.0.0-rc2) - Modern charting library for WPF
  - Alternative: OxyPlot.Wpf (2.1.2) - Mature charting library

**Recommendation**: Use LiveCharts2 for better WPF integration and modern API.

### File Structure

```
src/
├── ATSLiveMap.Core/
│   ├── Models/
│   │   ├── TelemetryData.cs (MODIFY - add fuel, damage fields)
│   │   ├── PerformanceMetrics.cs (NEW)
│   │   ├── Trip.cs (NEW - Phase 2)
│   │   └── TripStatistics.cs (NEW - Phase 2)
│   ├── Interfaces/
│   │   ├── IPerformanceCalculator.cs (NEW)
│   │   ├── ITripRepository.cs (NEW - Phase 2)
│   │   └── ITripDetectionService.cs (NEW - Phase 2)
│   └── Services/
│       ├── PerformanceCalculator.cs (NEW)
│       ├── TripRepository.cs (NEW - Phase 2)
│       └── TripDetectionService.cs (NEW - Phase 2)
├── ATSLiveMap.Telemetry/
│   └── HttpTelemetryClient.cs (MODIFY - parse additional fields)
└── ATSLiveMap.UI/
    ├── ViewModels/
    │   └── PerformanceViewModel.cs (NEW)
    ├── Views/
    │   └── PerformanceWindow.xaml (NEW)
    ├── Converters/
    │   ├── GradeToColorConverter.cs (NEW)
    │   └── TrendToSymbolConverter.cs (NEW)
    └── App.xaml.cs (MODIFY - register new services)
```

## Configuration

### App Settings

Add to `appsettings.json`:
```json
{
  "Performance": {
    "SmoothnessAccelerationThreshold": 5.0,
    "SmoothnessBrakingThreshold": 10.0,
    "FuelEfficiencyBaseline": 6.0,
    "HighwaySpeedLimit": 65.0,
    "DefaultSpeedLimit": 55.0,
    "TripEndTimeoutMinutes": 5.0,
    "DatabasePath": "data/trips.db"
  }
}
```

### Color Scheme

Define in `App.xaml` resources:
```xml
<Color x:Key="GradeA">#27AE60</Color>
<Color x:Key="GradeB">#3498DB</Color>
<Color x:Key="GradeC">#F39C12</Color>
<Color x:Key="GradeD">#E74C3C</Color>
<Color x:Key="BackgroundLight">#ECF0F1</Color>
<Color x:Key="CardBackground">#FFFFFF</Color>
```

## Performance Considerations

### Update Frequency

- Telemetry updates: 100ms (10 Hz) - existing
- Metric calculations: Every telemetry update
- UI updates: 60 FPS (16.67ms) - existing WPF rendering loop
- Database writes: Only on trip end (minimize I/O)

### Memory Management

- Limit stored acceleration history to last 60 seconds (600 data points)
- Recent trips list: Load only 20 most recent by default
- Chart data: Limit to last 10 trips for line graph
- Dispose database connections properly

### Thread Safety

- PerformanceCalculator: Thread-safe (lock on state updates)
- TripRepository: Async/await pattern for database operations
- ViewModel updates: Dispatch to UI thread using `Dispatcher.Invoke()`

## Phase Implementation Notes

### Phase 1: Real-Time Scoring (MVP)

**Goal**: Display live performance metrics without persistence

**Scope**:
- Implement PerformanceCalculator service
- Create PerformanceViewModel and PerformanceWindow
- Display current session metrics in card layout
- Show overall grade with trend indicator
- NO database, NO trip history, NO charts

**Success Criteria**:
- Metrics update in real-time while driving
- Scores reflect driving behavior accurately
- UI is responsive and visually appealing
- Can run alongside existing map window

### Phase 2: History & Persistence

**Goal**: Save trips and display history

**Scope**:
- Add SQLite database with Trip table
- Implement TripRepository and TripDetectionService
- Detect trip start/end automatically
- Display recent trips list
- Show summary statistics

**Success Criteria**:
- Trips save automatically
- Can view past trip details
- Statistics calculate correctly
- Data persists across app restarts

### Phase 3: Analytics & Polish

**Goal**: Visual analytics and export

**Scope**:
- Add LiveCharts2 for graphing
- Implement performance trend line chart
- Implement comparison bar chart
- Add CSV export functionality
- Polish UI styling and animations

**Success Criteria**:
- Charts display performance trends clearly
- Can export all trip data to CSV
- UI is polished and professional
- Trend indicators are accurate
