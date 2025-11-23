# Design Document

## Overview

The ATS Live Map Desktop application is a standalone Windows desktop application that displays real-time truck position and heading on an interactive map of the American Truck Simulator game world. The application is designed for web developers transitioning to desktop development, prioritizing clear architecture, maintainability, and extensibility.

### Technology Stack Selection

**Chosen Stack: C# with WPF (Windows Presentation Foundation)**

**Rationale:**
- **Familiar Concepts**: WPF uses XAML (similar to HTML) and data binding (similar to modern web frameworks like React/Vue)
- **Rich Graphics**: Built-in support for hardware-accelerated 2D graphics, transforms, and animations
- **Mature Ecosystem**: Extensive documentation, NuGet packages, and community support
- **Native Performance**: True native Windows application with excellent performance
- **Learning Curve**: Gentler than C++ while more powerful than Electron for graphics-intensive applications
- **Tooling**: Visual Studio provides excellent debugging, IntelliSense, and XAML designer

**Alternative Considered: Electron/Tauri**
- Pros: Leverages existing web skills directly
- Cons: Higher memory footprint, less efficient for real-time graphics rendering, more complex native module integration

### High-Level Architecture

The application follows a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        UI Layer (WPF)                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Map Canvas   │  │ Player Marker│  │ Controls     │     │
│  │ Rendering    │  │ Overlay      │  │ (Zoom/Pan)   │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└────────────────────────┬────────────────────────────────────┘
                         │ Data Binding / Events
┌────────────────────────▼────────────────────────────────────┐
│                   Application Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Map Service  │  │ Coordinate   │  │ State        │     │
│  │              │  │ Projection   │  │ Manager      │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└────────────────────────┬────────────────────────────────────┘
                         │ Interface Contracts
┌────────────────────────▼────────────────────────────────────┐
│                   Telemetry Layer                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Telemetry    │  │ Shared Memory│  │ Data Models  │     │
│  │ Reader       │  │ Client       │  │              │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└────────────────────────┬────────────────────────────────────┘
                         │ Memory-Mapped File
┌────────────────────────▼────────────────────────────────────┐
│              ATS Game + Telemetry Plugin                    │
│                    (scs-sdk-plugin)                         │
└─────────────────────────────────────────────────────────────┘
```



## Architecture

### Component Overview

#### 1. Telemetry Layer

**Purpose**: Reads real-time game data from the ATS telemetry plugin via shared memory.

**Key Components**:
- `ITelemetryClient`: Interface defining telemetry data access contract
- `SharedMemoryTelemetryClient`: Implementation that reads from memory-mapped file
- `TelemetryData`: Data model containing position, heading, speed, and other game state
- `TelemetryPoller`: Background service that polls telemetry at configurable intervals

**Communication Mechanism**: 
- Uses Windows Memory-Mapped Files to read shared memory region created by the telemetry plugin
- The plugin (scs-sdk-plugin) creates a named shared memory region: `Local\SCSTelemetry`
- Data structure is defined by the plugin's telemetry format (JSON or binary)

**Design Decisions**:
- Polling interval: 50ms (20 updates/second) to exceed the 10 Hz minimum requirement
- Async/await pattern for non-blocking reads
- Automatic reconnection logic with exponential backoff (1s, 2s, 4s, max 5s)

#### 2. Application Layer

**Purpose**: Transforms telemetry data into application-specific models and manages application state.

**Key Components**:

**MapService**:
- Loads and manages map imagery (single image or tile set)
- Provides map metadata (dimensions, bounds, reference points)
- Handles map asset caching

**CoordinateProjection**:
- Converts ATS world coordinates (X, Y, Z) to map pixel coordinates
- Implements affine transformation with calibration points
- Handles coordinate system differences (ATS uses Z-up, maps use Y-down)

**StateManager**:
- Maintains current player state (position, heading, speed)
- Implements position smoothing/interpolation
- Publishes state changes via events or observable pattern
- Manages connection status and error states

**PlayerMarkerService**:
- Calculates marker position and rotation
- Applies smoothing algorithms (linear interpolation or Kalman filter)
- Manages marker visibility and scaling at different zoom levels

#### 3. UI Layer

**Purpose**: Renders the map, player marker, and interactive controls.

**Key Components**:

**MainWindow (XAML + Code-behind)**:
- Application window with menu bar and status bar
- Hosts the MapCanvas control
- Displays connection status and diagnostic information

**MapCanvas (Custom WPF Control)**:
- Renders map image using WPF Image or WriteableBitmap
- Implements pan and zoom via RenderTransform
- Handles mouse events for interaction
- Draws player marker overlay

**PlayerMarkerOverlay**:
- Custom visual element drawn on top of map
- Rotates based on heading angle
- Scales appropriately with zoom level
- High contrast design for visibility



### Telemetry Plugin Selection

**Recommended Plugin: scs-sdk-plugin**

**Repository**: https://github.com/RenCloud/scs-sdk-plugin

**Rationale**:
- Community-maintained and actively updated
- No custom C++ development required
- Uses shared memory for efficient IPC
- Outputs telemetry data in JSON format (easy to parse)
- Well-documented installation process
- Supports both ATS and ETS2

**Installation Process**:
1. Download latest release DLL from GitHub releases
2. Place DLL in: `Documents\American Truck Simulator\bin\win_x64\plugins\`
3. Create plugins folder if it doesn't exist
4. Launch ATS - plugin loads automatically
5. Verify: Check `game.log.txt` for plugin initialization message

**Data Format**:
The plugin creates a memory-mapped file containing JSON data updated at ~20 Hz:
```json
{
  "game": {
    "connected": true,
    "gameName": "American Truck Simulator",
    "paused": false
  },
  "truck": {
    "position": {
      "X": 12345.67,
      "Y": 89.12,
      "Z": -23456.78
    },
    "orientation": {
      "heading": 1.5708,
      "pitch": 0.0,
      "roll": 0.0
    },
    "speed": 25.5
  }
}
```

**Alternative: Custom Plugin Development**

If building a custom plugin is desired:
- Requires C++ development environment (Visual Studio 2019+)
- Use SCS Telemetry SDK from: https://github.com/RenCloud/scs-sdk-plugin/tree/master/scs-telemetry
- Implement telemetry callbacks for position and orientation events
- Create shared memory region and serialize data
- Compile as 64-bit DLL
- More control but significantly higher complexity



## Components and Interfaces

### Telemetry Layer Interfaces

```csharp
// Core telemetry data model
public class TelemetryData
{
    public bool IsConnected { get; set; }
    public string GameName { get; set; }
    public bool IsPaused { get; set; }
    public Vector3 Position { get; set; }  // X, Y, Z in game world coordinates
    public float Heading { get; set; }      // Radians, 0 = North, increases clockwise
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Speed { get; set; }        // m/s
    public DateTime Timestamp { get; set; }
}

// Telemetry client interface
public interface ITelemetryClient
{
    bool IsConnected { get; }
    TelemetryData GetCurrentData();
    Task<TelemetryData> GetCurrentDataAsync();
    event EventHandler<TelemetryData> DataUpdated;
    event EventHandler<string> ConnectionStatusChanged;
    Task StartAsync();
    Task StopAsync();
}

// Shared memory implementation
public class SharedMemoryTelemetryClient : ITelemetryClient
{
    private const string MemoryMapName = "Local\\SCSTelemetry";
    private MemoryMappedFile _memoryMappedFile;
    private Timer _pollingTimer;
    private TelemetryData _lastData;
    
    // Implementation details in code examples section
}
```

### Application Layer Interfaces

```csharp
// Coordinate projection service
public interface ICoordinateProjection
{
    Point WorldToMap(Vector3 worldPosition);
    Vector3 MapToWorld(Point mapPosition);
    void Calibrate(List<CalibrationPoint> referencePoints);
}

public class CalibrationPoint
{
    public string LocationName { get; set; }
    public Vector3 WorldPosition { get; set; }
    public Point MapPixelPosition { get; set; }
}

// Affine transformation implementation
public class AffineCoordinateProjection : ICoordinateProjection
{
    private Matrix _transformMatrix;
    
    public void Calibrate(List<CalibrationPoint> referencePoints)
    {
        // Solve for affine transformation matrix using least squares
        // Handles rotation, scale, and translation
    }
    
    public Point WorldToMap(Vector3 worldPosition)
    {
        // Apply transformation: [x', y'] = M * [x, z] + [tx, ty]
        // Note: ATS uses Y as vertical, we map X,Z to map X,Y
    }
}

// Map service interface
public interface IMapService
{
    Task<BitmapImage> LoadMapAsync();
    MapMetadata GetMetadata();
    Task<BitmapImage> GetTileAsync(int x, int y, int zoom);
}

public class MapMetadata
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<CalibrationPoint> ReferencePoints { get; set; }
    public bool IsTiled { get; set; }
    public int TileSize { get; set; }
}

// State manager
public interface IStateManager
{
    PlayerState CurrentState { get; }
    ConnectionStatus Status { get; }
    event EventHandler<PlayerState> StateUpdated;
    void UpdateFromTelemetry(TelemetryData data);
}

public class PlayerState
{
    public Point MapPosition { get; set; }
    public float Heading { get; set; }
    public float Speed { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Smoothed values for rendering
    public Point SmoothedMapPosition { get; set; }
    public float SmoothedHeading { get; set; }
}
```



### UI Layer Components

```csharp
// Main window view model (MVVM pattern)
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IStateManager _stateManager;
    private readonly ITelemetryClient _telemetryClient;
    
    public PlayerState CurrentPlayer { get; private set; }
    public string ConnectionStatus { get; private set; }
    public bool IsConnected { get; private set; }
    
    // Commands for UI interactions
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand CenterOnPlayerCommand { get; }
    public ICommand ToggleDiagnosticsCommand { get; }
}

// Map canvas control
public class MapCanvas : FrameworkElement
{
    // Dependency properties for data binding
    public static readonly DependencyProperty MapImageProperty;
    public static readonly DependencyProperty PlayerPositionProperty;
    public static readonly DependencyProperty PlayerHeadingProperty;
    public static readonly DependencyProperty ZoomLevelProperty;
    
    // Transform properties
    private TransformGroup _transformGroup;
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    
    // Interaction state
    private Point _lastMousePosition;
    private bool _isPanning;
    
    protected override void OnRender(DrawingContext dc)
    {
        // Render map image
        // Render player marker with rotation
        // Apply transforms for zoom and pan
    }
}
```



## Data Models

### Core Data Structures

```csharp
// 3D Vector for world coordinates
public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }  // Vertical axis in ATS
    public float Z { get; set; }
    
    public static Vector3 Zero => new Vector3(0, 0, 0);
    
    public float Distance(Vector3 other)
    {
        float dx = X - other.X;
        float dy = Y - other.Y;
        float dz = Z - other.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

// Connection status enumeration
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

// Configuration model
public class AppConfiguration
{
    public string MapImagePath { get; set; }
    public int TelemetryPollingIntervalMs { get; set; } = 50;
    public int ReconnectIntervalMs { get; set; } = 2000;
    public float MinZoom { get; set; } = 0.25f;
    public float MaxZoom { get; set; } = 4.0f;
    public bool EnableDiagnostics { get; set; } = false;
    public string DiagnosticsLogPath { get; set; }
    
    // Smoothing parameters
    public float PositionSmoothingFactor { get; set; } = 0.3f;
    public float HeadingSmoothingFactor { get; set; } = 0.5f;
}

// Diagnostic data for troubleshooting
public class DiagnosticSnapshot
{
    public DateTime Timestamp { get; set; }
    public TelemetryData RawTelemetry { get; set; }
    public Point MapPosition { get; set; }
    public PlayerState SmoothedState { get; set; }
    public ConnectionStatus Status { get; set; }
    
    public string ToLogString()
    {
        return $"[{Timestamp:HH:mm:ss.fff}] " +
               $"World: ({RawTelemetry.Position.X:F2}, {RawTelemetry.Position.Z:F2}) " +
               $"Map: ({MapPosition.X:F0}, {MapPosition.Y:F0}) " +
               $"Heading: {RawTelemetry.Heading:F2} rad " +
               $"Status: {Status}";
    }
}
```

### Data Flow

```
ATS Game Engine
    ↓ (Game events)
Telemetry Plugin (DLL in game process)
    ↓ (Writes to shared memory ~20 Hz)
Shared Memory Region (JSON data)
    ↓ (Polling read)
SharedMemoryTelemetryClient
    ↓ (Deserialize & emit events)
StateManager
    ↓ (Apply coordinate projection)
CoordinateProjection
    ↓ (Apply smoothing)
StateManager.SmoothedState
    ↓ (Data binding / PropertyChanged)
MainViewModel
    ↓ (WPF data binding)
MapCanvas.OnRender()
    ↓ (Visual output)
Screen Display
```



## Map System Design

### Map Acquisition

**Option 1: Community Map Resources (Recommended for MVP)**

Use pre-rendered maps from the ATS community:
- **TruckersMP Map**: High-quality rendered maps available at https://map.truckers.mp/
- **Resolution**: 8192x8192 pixels or higher
- **Format**: PNG with transparency support
- **Content**: Cities, highways, state boundaries, city labels, and street names
- **License**: Check community guidelines for usage rights
- **Note**: TruckersMP maps include detailed street names for major roads and highways

**Option 2: Generate from Game Files**

Extract and render map from ATS game data:
- Requires SCS Extractor tool to unpack `.scs` archive files
- Map definition files located in: `def/city/`, `def/ferry/`, `def/road/`
- Road names are stored in localization files: `locale/en_us/`
- Requires custom rendering tool to convert definitions to visual map
- More complex but allows full customization including street name placement
- Reference: https://modding.scssoft.com/wiki/Documentation/Tools/Game_Archive_Packer

**Street Name Rendering**:
When generating custom maps or overlaying street names:
- Parse road definitions from `def/road/` to get road geometry
- Match road IDs to names from localization files
- Render text along road curves using path text rendering
- Apply appropriate font size based on road importance (Interstate > US Highway > State Road)
- Ensure text is readable at multiple zoom levels (may require level-of-detail system)

**Recommended Approach for This Project**:
Start with Option 1 (community map) for rapid development, then optionally implement Option 2 for customization.

### Map Storage Strategy

**Single Large Image (Recommended for MVP)**

**Pros**:
- Simpler implementation
- No tile management complexity
- Easier coordinate mapping
- Suitable for maps up to 16384x16384 pixels

**Cons**:
- Higher memory usage (~256 MB for 8192x8192 RGBA)
- Slower initial load time
- Limited scalability for very large maps

**Implementation**:
```csharp
public class SingleImageMapService : IMapService
{
    private BitmapImage _cachedMap;
    private readonly string _mapPath;
    
    public async Task<BitmapImage> LoadMapAsync()
    {
        if (_cachedMap != null) return _cachedMap;
        
        _cachedMap = new BitmapImage();
        _cachedMap.BeginInit();
        _cachedMap.UriSource = new Uri(_mapPath);
        _cachedMap.CacheOption = BitmapCacheOption.OnLoad;
        _cachedMap.EndInit();
        _cachedMap.Freeze(); // Make thread-safe
        
        return _cachedMap;
    }
}
```

**Tiled Map System (Future Enhancement)**

**Pros**:
- Lower memory footprint
- Faster initial load
- Scalable to very large maps
- Load tiles on-demand

**Cons**:
- More complex implementation
- Requires tile generation preprocessing
- More complex coordinate calculations

**Tile Structure**:
```
maps/
  ats/
    zoom_0/  (full map at 1:1)
      tile_0_0.png
      tile_0_1.png
      ...
    zoom_1/  (2x zoom)
      tile_0_0.png
      ...
```



### Coordinate Projection System

**Problem**: Convert ATS 3D world coordinates to 2D map pixel coordinates.

**ATS Coordinate System**:
- Origin: Arbitrary point in game world
- X-axis: East-West (positive = East)
- Y-axis: Vertical (positive = Up)
- Z-axis: North-South (positive = South)
- Units: Meters
- Range: Approximately -100,000 to +100,000 meters

**Map Coordinate System**:
- Origin: Top-left corner (0, 0)
- X-axis: Horizontal (positive = Right)
- Y-axis: Vertical (positive = Down)
- Units: Pixels
- Range: 0 to map width/height

**Transformation Approach: Affine Transformation**

An affine transformation preserves parallel lines and handles rotation, scale, and translation:

```
[x_map]   [a  b] [x_world]   [tx]
[y_map] = [c  d] [z_world] + [ty]
```

Where:
- `a, b, c, d` = transformation matrix (handles rotation and scale)
- `tx, ty` = translation offsets
- Note: We use X and Z from world coordinates (ignoring Y/altitude)

**Calibration Process**:

1. Identify at least 3 known reference points (cities):
   - Los Angeles: World(-87500, 0, -103000), Map(1250, 6800)
   - San Francisco: World(-91000, 0, -119000), Map(950, 5200)
   - Las Vegas: World(-76000, 0, -96000), Map(2100, 6200)
   - (Example coordinates - actual values need verification in-game)

2. Solve for transformation matrix using least-squares:
   ```csharp
   public void Calibrate(List<CalibrationPoint> points)
   {
       // Build matrices for least-squares solution
       // A * x = b, where x = [a, b, tx, c, d, ty]
       
       int n = points.Count;
       var matrixA = new double[2 * n, 6];
       var vectorB = new double[2 * n];
       
       for (int i = 0; i < n; i++)
       {
           var wp = points[i].WorldPosition;
           var mp = points[i].MapPixelPosition;
           
           // Equation for x_map
           matrixA[2 * i, 0] = wp.X;
           matrixA[2 * i, 1] = wp.Z;
           matrixA[2 * i, 2] = 1;
           vectorB[2 * i] = mp.X;
           
           // Equation for y_map
           matrixA[2 * i + 1, 3] = wp.X;
           matrixA[2 * i + 1, 4] = wp.Z;
           matrixA[2 * i + 1, 5] = 1;
           vectorB[2 * i + 1] = mp.Y;
       }
       
       // Solve using Math.NET Numerics or similar library
       var solution = SolveLeastSquares(matrixA, vectorB);
       
       _transformMatrix = new Matrix(
           solution[0], solution[1], solution[2],
           solution[3], solution[4], solution[5]
       );
   }
   ```

3. Apply transformation for any world position:
   ```csharp
   public Point WorldToMap(Vector3 worldPos)
   {
       double x = _transformMatrix.M11 * worldPos.X + 
                  _transformMatrix.M12 * worldPos.Z + 
                  _transformMatrix.M13;
                  
       double y = _transformMatrix.M21 * worldPos.X + 
                  _transformMatrix.M22 * worldPos.Z + 
                  _transformMatrix.M23;
       
       return new Point(x, y);
   }
   ```

**Worked Example**:

Given calibration points (simplified):
- Point A: World(0, 0, 0) → Map(4000, 4000)
- Point B: World(10000, 0, 0) → Map(5000, 4000)
- Point C: World(0, 0, 10000) → Map(4000, 3000)

This suggests:
- Scale: 0.1 pixels per meter
- X increases right on map
- Z increases up on map (inverted from typical)

Transform: `x_map = 0.1 * x_world + 4000`, `y_map = -0.1 * z_world + 4000`

For truck at World(5000, 0, 5000):
- `x_map = 0.1 * 5000 + 4000 = 4500`
- `y_map = -0.1 * 5000 + 4000 = 3500`



### Interactive Map Controls

**Pan Implementation**:

```csharp
private void MapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
{
    if (e.LeftButton == MouseButtonState.Pressed)
    {
        _isPanning = true;
        _lastMousePosition = e.GetPosition(this);
        CaptureMouse();
    }
}

private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
{
    if (_isPanning)
    {
        Point currentPosition = e.GetPosition(this);
        Vector delta = currentPosition - _lastMousePosition;
        
        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;
        
        _lastMousePosition = currentPosition;
        InvalidateVisual(); // Trigger re-render
    }
}

private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
{
    if (_isPanning)
    {
        _isPanning = false;
        ReleaseMouseCapture();
    }
}
```

**Zoom Implementation**:

```csharp
private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
{
    Point mousePos = e.GetPosition(this);
    
    // Calculate zoom factor
    double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
    double newZoom = _scaleTransform.ScaleX * zoomFactor;
    
    // Clamp zoom level
    newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));
    
    // Zoom toward mouse cursor
    // Adjust translation to keep mouse point stationary
    double offsetX = mousePos.X - _translateTransform.X;
    double offsetY = mousePos.Y - _translateTransform.Y;
    
    _translateTransform.X = mousePos.X - offsetX * (newZoom / _scaleTransform.ScaleX);
    _translateTransform.Y = mousePos.Y - offsetY * (newZoom / _scaleTransform.ScaleY);
    
    _scaleTransform.ScaleX = newZoom;
    _scaleTransform.ScaleY = newZoom;
    
    InvalidateVisual();
}
```

**Center on Player**:

```csharp
public void CenterOnPlayer()
{
    if (CurrentPlayerState == null) return;
    
    Point playerMapPos = CurrentPlayerState.MapPosition;
    
    // Calculate translation to center player in viewport
    _translateTransform.X = (ActualWidth / 2) - (playerMapPos.X * _scaleTransform.ScaleX);
    _translateTransform.Y = (ActualHeight / 2) - (playerMapPos.Y * _scaleTransform.ScaleY);
    
    InvalidateVisual();
}
```



### Player Marker Rendering

**Marker Design**:

Visual representation: Truck icon (triangle or custom SVG) with directional indicator.

```csharp
protected override void OnRender(DrawingContext dc)
{
    base.OnRender(dc);
    
    // Render map image
    if (MapImage != null)
    {
        dc.DrawImage(MapImage, new Rect(0, 0, MapImage.Width, MapImage.Height));
    }
    
    // Render player marker
    if (PlayerPosition != null && IsConnected)
    {
        DrawPlayerMarker(dc, PlayerPosition, PlayerHeading);
    }
}

private void DrawPlayerMarker(DrawingContext dc, Point position, float headingRadians)
{
    // Convert heading to degrees (0 = North, clockwise)
    double headingDegrees = headingRadians * (180.0 / Math.PI);
    
    // Create marker geometry (triangle pointing up)
    var markerGeometry = new StreamGeometry();
    using (var ctx = markerGeometry.Open())
    {
        ctx.BeginFigure(new Point(0, -20), true, true);  // Top point
        ctx.LineTo(new Point(-10, 10), true, true);       // Bottom left
        ctx.LineTo(new Point(10, 10), true, true);        // Bottom right
    }
    markerGeometry.Freeze();
    
    // Apply transforms: translate to position, then rotate
    var transformGroup = new TransformGroup();
    transformGroup.Children.Add(new RotateTransform(headingDegrees));
    transformGroup.Children.Add(new TranslateTransform(position.X, position.Y));
    
    // Draw marker with outline for visibility
    dc.PushTransform(transformGroup);
    dc.DrawGeometry(Brushes.Red, new Pen(Brushes.White, 2), markerGeometry);
    dc.Pop();
    
    // Draw position dot at exact location
    dc.DrawEllipse(Brushes.Yellow, new Pen(Brushes.Black, 1), 
                   position, 3, 3);
}
```

**Marker Scaling with Zoom**:

```csharp
private void DrawPlayerMarker(DrawingContext dc, Point position, float headingRadians)
{
    // Scale marker inversely with zoom to maintain constant screen size
    double markerScale = 1.0 / _scaleTransform.ScaleX;
    markerScale = Math.Max(0.5, Math.Min(2.0, markerScale)); // Clamp
    
    var transformGroup = new TransformGroup();
    transformGroup.Children.Add(new ScaleTransform(markerScale, markerScale));
    transformGroup.Children.Add(new RotateTransform(headingRadians * 180 / Math.PI));
    transformGroup.Children.Add(new TranslateTransform(position.X, position.Y));
    
    dc.PushTransform(transformGroup);
    // ... draw geometry
    dc.Pop();
}
```



## Error Handling

### Connection Error Handling

```csharp
public class SharedMemoryTelemetryClient : ITelemetryClient
{
    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    
    public async Task StartAsync()
    {
        while (true)
        {
            try
            {
                _status = ConnectionStatus.Connecting;
                OnConnectionStatusChanged("Connecting to ATS...");
                
                _memoryMappedFile = MemoryMappedFile.OpenExisting(MemoryMapName);
                
                _status = ConnectionStatus.Connected;
                OnConnectionStatusChanged("Connected to ATS");
                
                // Start polling
                _pollingTimer = new Timer(PollTelemetry, null, 0, PollingIntervalMs);
                break;
            }
            catch (FileNotFoundException)
            {
                _status = ConnectionStatus.Disconnected;
                OnConnectionStatusChanged("Waiting for ATS to start...");
                await Task.Delay(ReconnectIntervalMs);
            }
            catch (Exception ex)
            {
                _status = ConnectionStatus.Error;
                OnConnectionStatusChanged($"Error: {ex.Message}");
                await Task.Delay(ReconnectIntervalMs);
            }
        }
    }
    
    private void PollTelemetry(object state)
    {
        try
        {
            using (var accessor = _memoryMappedFile.CreateViewAccessor())
            {
                byte[] buffer = new byte[accessor.Capacity];
                accessor.ReadArray(0, buffer, 0, buffer.Length);
                
                string json = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                var data = JsonSerializer.Deserialize<TelemetryData>(json);
                
                OnDataUpdated(data);
            }
        }
        catch (Exception ex)
        {
            _status = ConnectionStatus.Error;
            OnConnectionStatusChanged($"Read error: {ex.Message}");
            
            // Attempt reconnection
            _pollingTimer?.Dispose();
            _ = StartAsync();
        }
    }
}
```

### Coordinate Validation

```csharp
public Point WorldToMap(Vector3 worldPosition)
{
    Point mapPos = ApplyTransform(worldPosition);
    
    // Validate coordinates are within map bounds
    if (mapPos.X < 0 || mapPos.X > _mapWidth ||
        mapPos.Y < 0 || mapPos.Y > _mapHeight)
    {
        Logger.Warning($"Position out of bounds: World{worldPosition} -> Map{mapPos}");
        
        // Clamp to map edges
        mapPos.X = Math.Max(0, Math.Min(_mapWidth, mapPos.X));
        mapPos.Y = Math.Max(0, Math.Min(_mapHeight, mapPos.Y));
    }
    
    return mapPos;
}
```

### Map Loading Error Handling

```csharp
public async Task<BitmapImage> LoadMapAsync()
{
    try
    {
        if (!File.Exists(_mapPath))
        {
            throw new FileNotFoundException(
                $"Map file not found at: {_mapPath}\n\n" +
                $"Please ensure the map image is placed in the correct location.\n" +
                $"Expected path: {Path.GetFullPath(_mapPath)}"
            );
        }
        
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(_mapPath, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        
        return bitmap;
    }
    catch (Exception ex)
    {
        Logger.Error($"Failed to load map: {ex.Message}");
        
        // Show error dialog to user
        MessageBox.Show(
            $"Failed to load map image:\n{ex.Message}\n\n" +
            $"The application will continue but the map will not be displayed.",
            "Map Loading Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
        
        return null;
    }
}
```

### User-Facing Error Messages

```csharp
public class ErrorMessageProvider
{
    public static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException => 
                "The telemetry plugin is not running.\n\n" +
                "Please ensure:\n" +
                "1. ATS is running\n" +
                "2. The scs-sdk-plugin DLL is in the plugins folder\n" +
                "3. Check game.log.txt for plugin loading errors",
                
            UnauthorizedAccessException =>
                "Permission denied accessing telemetry data.\n\n" +
                "Try running the application as administrator.",
                
            JsonException =>
                "Invalid telemetry data format.\n\n" +
                "The plugin may be outdated or incompatible.",
                
            _ => $"An unexpected error occurred:\n{ex.Message}"
        };
    }
}
```



## Testing Strategy

### Unit Testing

**Test Framework**: xUnit or NUnit

**Key Test Areas**:

1. **Coordinate Projection Tests**:
```csharp
[Fact]
public void WorldToMap_WithKnownCalibrationPoints_ReturnsCorrectMapPosition()
{
    // Arrange
    var projection = new AffineCoordinateProjection();
    var calibrationPoints = new List<CalibrationPoint>
    {
        new() { WorldPosition = new Vector3(0, 0, 0), MapPixelPosition = new Point(4000, 4000) },
        new() { WorldPosition = new Vector3(10000, 0, 0), MapPixelPosition = new Point(5000, 4000) },
        new() { WorldPosition = new Vector3(0, 0, 10000), MapPixelPosition = new Point(4000, 3000) }
    };
    projection.Calibrate(calibrationPoints);
    
    // Act
    var result = projection.WorldToMap(new Vector3(5000, 0, 5000));
    
    // Assert
    Assert.InRange(result.X, 4450, 4550); // Within 50 pixels
    Assert.InRange(result.Y, 3450, 3550);
}

[Fact]
public void WorldToMap_WithOutOfBoundsCoordinates_ClampsToMapEdges()
{
    // Test boundary handling
}
```

2. **Telemetry Data Parsing Tests**:
```csharp
[Fact]
public void ParseTelemetryJson_WithValidData_ReturnsCorrectValues()
{
    // Arrange
    string json = @"{
        ""truck"": {
            ""position"": { ""X"": 1000.5, ""Y"": 50.0, ""Z"": -2000.5 },
            ""orientation"": { ""heading"": 1.5708 }
        }
    }";
    
    // Act
    var data = TelemetryParser.Parse(json);
    
    // Assert
    Assert.Equal(1000.5f, data.Position.X, 2);
    Assert.Equal(1.5708f, data.Heading, 4);
}
```

3. **Smoothing Algorithm Tests**:
```csharp
[Fact]
public void ApplySmoothing_WithJitteryInput_ProducesSmoothOutput()
{
    // Test position interpolation
}
```

### Integration Testing

1. **Telemetry Client Integration**:
   - Mock shared memory with test data
   - Verify polling and event emission
   - Test reconnection logic

2. **End-to-End Coordinate Flow**:
   - Inject test telemetry data
   - Verify transformation through all layers
   - Validate final screen position

### Manual Testing Checklist

- [ ] Plugin loads successfully in ATS
- [ ] Application connects to telemetry on startup
- [ ] Player marker appears at correct location
- [ ] Marker rotates with truck heading
- [ ] Pan with mouse drag works smoothly
- [ ] Zoom with mouse wheel works smoothly
- [ ] Zoom centers on mouse cursor
- [ ] Center on player button works
- [ ] Connection status displays correctly
- [ ] Error messages appear when ATS not running
- [ ] Application reconnects when ATS restarts
- [ ] Performance: 30+ FPS during movement
- [ ] Performance: <10% CPU when static
- [ ] Memory usage stable over time

### Performance Testing

**Metrics to Monitor**:
- Frame rate during rendering (target: 30+ FPS)
- Telemetry polling latency (target: <50ms)
- Memory usage (target: <500 MB)
- CPU usage when idle (target: <10%)

**Profiling Tools**:
- Visual Studio Performance Profiler
- WPF Performance Suite
- Windows Performance Monitor



## Performance Optimization

### Threading Strategy

**UI Thread**:
- Map rendering
- User input handling
- Data binding updates

**Background Thread (Telemetry Polling)**:
```csharp
public class TelemetryPoller
{
    private readonly ITelemetryClient _client;
    private readonly IStateManager _stateManager;
    private CancellationTokenSource _cts;
    
    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        
        await Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var data = await _client.GetCurrentDataAsync();
                    
                    // Update state on UI thread
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _stateManager.UpdateFromTelemetry(data);
                    });
                    
                    await Task.Delay(50, _cts.Token); // 20 Hz
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, _cts.Token);
    }
}
```

### Rendering Optimization

**Dirty Region Tracking**:
```csharp
public class MapCanvas : FrameworkElement
{
    private bool _isMapDirty = true;
    private bool _isMarkerDirty = true;
    
    protected override void OnRender(DrawingContext dc)
    {
        // Only re-render map if zoom/pan changed
        if (_isMapDirty)
        {
            RenderMap(dc);
            _isMapDirty = false;
        }
        
        // Always render marker (updates frequently)
        RenderPlayerMarker(dc);
    }
    
    private void OnZoomChanged()
    {
        _isMapDirty = true;
        InvalidateVisual();
    }
}
```

**Image Caching**:
```csharp
public class MapService
{
    private readonly Dictionary<string, BitmapImage> _imageCache = new();
    
    public BitmapImage GetCachedImage(string path)
    {
        if (_imageCache.TryGetValue(path, out var cached))
            return cached;
        
        var image = LoadImage(path);
        image.Freeze(); // Make immutable and thread-safe
        _imageCache[path] = image;
        
        return image;
    }
}
```

### Position Smoothing

**Linear Interpolation (Simple)**:
```csharp
public class LinearSmoother
{
    private Point _lastPosition;
    private float _smoothingFactor = 0.3f; // 0 = no smoothing, 1 = instant
    
    public Point Smooth(Point newPosition)
    {
        if (_lastPosition == default)
        {
            _lastPosition = newPosition;
            return newPosition;
        }
        
        Point smoothed = new Point(
            _lastPosition.X + (newPosition.X - _lastPosition.X) * _smoothingFactor,
            _lastPosition.Y + (newPosition.Y - _lastPosition.Y) * _smoothingFactor
        );
        
        _lastPosition = smoothed;
        return smoothed;
    }
}
```

**Kalman Filter (Advanced)**:
```csharp
public class KalmanSmoother
{
    private double _estimate;
    private double _errorCovariance = 1.0;
    private readonly double _processNoise = 0.01;
    private readonly double _measurementNoise = 0.1;
    
    public double Update(double measurement)
    {
        // Prediction
        double predictedEstimate = _estimate;
        double predictedCovariance = _errorCovariance + _processNoise;
        
        // Update
        double kalmanGain = predictedCovariance / (predictedCovariance + _measurementNoise);
        _estimate = predictedEstimate + kalmanGain * (measurement - predictedEstimate);
        _errorCovariance = (1 - kalmanGain) * predictedCovariance;
        
        return _estimate;
    }
}
```

### Memory Management

**Dispose Pattern**:
```csharp
public class SharedMemoryTelemetryClient : ITelemetryClient, IDisposable
{
    private MemoryMappedFile _memoryMappedFile;
    private Timer _pollingTimer;
    private bool _disposed;
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _pollingTimer?.Dispose();
        _memoryMappedFile?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```

**Weak Event Pattern** (prevent memory leaks):
```csharp
public class StateManager : IStateManager
{
    private readonly WeakEventManager _eventManager = new();
    
    public event EventHandler<PlayerState> StateUpdated
    {
        add => _eventManager.AddHandler(value);
        remove => _eventManager.RemoveHandler(value);
    }
    
    protected void OnStateUpdated(PlayerState state)
    {
        _eventManager.RaiseEvent(this, state, nameof(StateUpdated));
    }
}
```



## Development Environment Setup

### Required Software

1. **Visual Studio 2022 Community Edition** (Free)
   - Version: 17.8 or later
   - Workloads: ".NET desktop development"
   - Download: https://visualstudio.microsoft.com/downloads/

2. **.NET SDK**
   - Version: .NET 8.0 or later
   - Included with Visual Studio
   - Standalone: https://dotnet.microsoft.com/download

3. **American Truck Simulator**
   - Version: 1.49 or later
   - Platform: Steam or standalone
   - Required for testing

4. **scs-sdk-plugin**
   - Latest release from: https://github.com/RenCloud/scs-sdk-plugin/releases
   - Download the DLL file (e.g., `scs-telemetry.dll`)

### Optional Tools

1. **Git** for version control
2. **NuGet Package Manager** (included in Visual Studio)
3. **SCS Extractor** (if generating custom maps)
4. **Image editing software** (GIMP, Photoshop) for map customization

### NuGet Packages

```xml
<ItemGroup>
  <!-- JSON serialization -->
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
  
  <!-- Math library for coordinate calculations -->
  <PackageReference Include="MathNet.Numerics" Version="5.0.0" />
  
  <!-- Logging -->
  <PackageReference Include="Serilog" Version="3.1.1" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  
  <!-- Testing -->
  <PackageReference Include="xunit" Version="2.6.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  <PackageReference Include="Moq" Version="4.20.0" />
</ItemGroup>
```

### Project Structure

```
ATSLiveMap/
├── ATSLiveMap.sln                    # Solution file
├── src/
│   ├── ATSLiveMap.Core/              # Core business logic
│   │   ├── Models/
│   │   │   ├── TelemetryData.cs
│   │   │   ├── PlayerState.cs
│   │   │   └── Vector3.cs
│   │   ├── Interfaces/
│   │   │   ├── ITelemetryClient.cs
│   │   │   ├── ICoordinateProjection.cs
│   │   │   ├── IMapService.cs
│   │   │   └── IStateManager.cs
│   │   ├── Services/
│   │   │   ├── CoordinateProjection.cs
│   │   │   ├── MapService.cs
│   │   │   ├── StateManager.cs
│   │   │   └── PositionSmoother.cs
│   │   └── ATSLiveMap.Core.csproj
│   │
│   ├── ATSLiveMap.Telemetry/         # Telemetry layer
│   │   ├── SharedMemoryTelemetryClient.cs
│   │   ├── TelemetryPoller.cs
│   │   ├── TelemetryParser.cs
│   │   └── ATSLiveMap.Telemetry.csproj
│   │
│   └── ATSLiveMap.UI/                # WPF application
│       ├── Views/
│       │   ├── MainWindow.xaml
│       │   ├── MainWindow.xaml.cs
│       │   └── Controls/
│       │       ├── MapCanvas.cs
│       │       └── StatusBar.xaml
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   └── ViewModelBase.cs
│       ├── Resources/
│       │   ├── Icons/
│       │   │   └── truck-marker.png
│       │   └── Styles/
│       │       └── AppStyles.xaml
│       ├── App.xaml
│       ├── App.xaml.cs
│       └── ATSLiveMap.UI.csproj
│
├── tests/
│   ├── ATSLiveMap.Core.Tests/
│   │   ├── CoordinateProjectionTests.cs
│   │   ├── StateManagerTests.cs
│   │   └── ATSLiveMap.Core.Tests.csproj
│   └── ATSLiveMap.Telemetry.Tests/
│       ├── TelemetryParserTests.cs
│       └── ATSLiveMap.Telemetry.Tests.csproj
│
├── assets/
│   ├── maps/
│   │   └── ats-map.png              # Map image file
│   └── config/
│       └── calibration.json         # Calibration points
│
├── docs/
│   ├── setup-guide.md
│   ├── troubleshooting.md
│   └── architecture.md
│
└── README.md
```

### Configuration Files

**appsettings.json**:
```json
{
  "Telemetry": {
    "PollingIntervalMs": 50,
    "ReconnectIntervalMs": 2000,
    "MemoryMapName": "Local\\SCSTelemetry"
  },
  "Map": {
    "ImagePath": "assets/maps/ats-map.png",
    "Width": 8192,
    "Height": 8192
  },
  "Rendering": {
    "MinZoom": 0.25,
    "MaxZoom": 4.0,
    "TargetFrameRate": 30,
    "PositionSmoothingFactor": 0.3,
    "HeadingSmoothingFactor": 0.5
  },
  "Diagnostics": {
    "EnableLogging": true,
    "LogPath": "logs/telemetry.log",
    "LogLevel": "Information"
  }
}
```

**calibration.json**:
```json
{
  "referencePoints": [
    {
      "name": "Los Angeles",
      "worldPosition": { "x": -87500, "y": 0, "z": -103000 },
      "mapPixelPosition": { "x": 1250, "y": 6800 }
    },
    {
      "name": "San Francisco",
      "worldPosition": { "x": -91000, "y": 0, "z": -119000 },
      "mapPixelPosition": { "x": 950, "y": 5200 }
    },
    {
      "name": "Las Vegas",
      "worldPosition": { "x": -76000, "y": 0, "z": -96000 },
      "mapPixelPosition": { "x": 2100, "y": 6200 }
    }
  ]
}
```



## Build and Deployment

### Build Configuration

**Debug Configuration**:
- Enables diagnostic logging
- Includes debug symbols
- No code optimization
- Used during development

**Release Configuration**:
- Optimized code
- Minimal logging
- Smaller executable size
- Used for distribution

### Build Commands

```bash
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build ATSLiveMap.sln --configuration Release

# Run tests
dotnet test

# Publish self-contained executable
dotnet publish src/ATSLiveMap.UI/ATSLiveMap.UI.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./publish
```

### Deployment Package

The published application should include:
```
ATSLiveMap/
├── ATSLiveMap.UI.exe          # Main executable
├── *.dll                       # Dependencies
├── assets/
│   ├── maps/
│   │   └── ats-map.png
│   └── config/
│       └── calibration.json
├── appsettings.json
├── README.txt                  # User instructions
└── SETUP.txt                   # Plugin installation guide
```

### Installation Instructions for End Users

1. Extract ZIP file to desired location
2. Install telemetry plugin:
   - Download `scs-telemetry.dll` from provided link
   - Copy to: `Documents\American Truck Simulator\bin\win_x64\plugins\`
   - Create `plugins` folder if it doesn't exist
3. Launch ATS to load the plugin
4. Run `ATSLiveMap.UI.exe`
5. Start driving in ATS - map should show your position



## Future Enhancements

### Phase 2: Job Information Display

**Features**:
- Current cargo type and weight
- Source and destination cities
- Delivery deadline and ETA
- Distance remaining
- Revenue information

**Implementation**:
- Extend `TelemetryData` model with job fields
- Create `JobInfoPanel` WPF control
- Add overlay to main window
- Parse additional telemetry fields from plugin

**UI Mockup**:
```
┌─────────────────────────┐
│ Current Job             │
├─────────────────────────┤
│ Cargo: Electronics      │
│ Weight: 18,500 kg       │
│ From: Los Angeles       │
│ To: Phoenix             │
│ Distance: 385 mi        │
│ ETA: 2h 15m             │
│ Revenue: $12,450        │
└─────────────────────────┘
```

### Phase 3: Truck Statistics HUD

**Features**:
- Speed (MPH/KPH)
- RPM gauge
- Fuel level and range
- Damage indicators
- Gear indicator

**Implementation**:
- Create `TruckStatsPanel` control
- Use circular gauges for RPM/speed
- Add warning indicators for low fuel/damage
- Position as overlay on map or separate panel

### Phase 4: Route Visualization

**Features**:
- Draw planned route on map
- Highlight current road segment
- Show waypoints and navigation markers
- Display route distance and time

**Implementation**:
- Parse navigation data from telemetry
- Convert waypoint coordinates to map positions
- Draw polyline connecting waypoints
- Implement route recalculation on deviation

**Technical Approach**:
```csharp
public class RouteRenderer
{
    public void DrawRoute(DrawingContext dc, List<Vector3> waypoints)
    {
        var mapPoints = waypoints.Select(wp => _projection.WorldToMap(wp)).ToList();
        
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(mapPoints[0], false, false);
            foreach (var point in mapPoints.Skip(1))
            {
                ctx.LineTo(point, true, false);
            }
        }
        
        dc.DrawGeometry(null, new Pen(Brushes.Blue, 3), geometry);
    }
}
```

### Phase 5: Explored Area Tracking

**Features**:
- Track roads/areas the player has visited
- Visualize explored vs unexplored regions
- Fog-of-war effect for unvisited areas
- Statistics: total distance driven, coverage percentage

**Implementation**:
- Maintain grid of visited cells
- Mark cells as explored when player passes through
- Render semi-transparent overlay for unexplored areas
- Persist exploration data to file

**Data Structure**:
```csharp
public class ExplorationTracker
{
    private readonly bool[,] _exploredGrid;
    private readonly int _cellSize = 100; // meters per cell
    
    public void MarkExplored(Vector3 position)
    {
        int gridX = (int)(position.X / _cellSize);
        int gridZ = (int)(position.Z / _cellSize);
        
        if (IsValidCell(gridX, gridZ))
        {
            _exploredGrid[gridX, gridZ] = true;
        }
    }
    
    public float GetExplorationPercentage()
    {
        int total = _exploredGrid.Length;
        int explored = _exploredGrid.Cast<bool>().Count(b => b);
        return (float)explored / total * 100;
    }
}
```

### Phase 6: Multi-Monitor Support

**Features**:
- Detect multiple monitors
- Allow map to be displayed on secondary monitor
- Full-screen mode for dedicated navigation display
- Remember window position across sessions

**Implementation**:
- Use `System.Windows.Forms.Screen` API
- Add "Move to Monitor" menu options
- Implement window position persistence
- Support borderless full-screen mode

### Phase 7: Dynamic Street Name Overlay

**Features**:
- Render street names dynamically based on zoom level
- Show/hide street names based on road importance
- Text follows road curves for better readability
- Level-of-detail system (show more detail at higher zoom)

**Implementation**:
```csharp
public class StreetNameRenderer
{
    private Dictionary<string, RoadGeometry> _roadData;
    
    public void RenderStreetNames(DrawingContext dc, double zoomLevel)
    {
        // Filter roads by importance and zoom level
        var visibleRoads = _roadData.Values
            .Where(road => ShouldRenderAtZoom(road.Importance, zoomLevel));
        
        foreach (var road in visibleRoads)
        {
            // Render text along path
            var formattedText = new FormattedText(
                road.Name,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                GetFontSize(road.Importance, zoomLevel),
                Brushes.DarkGray
            );
            
            // Draw text following road curve
            DrawTextAlongPath(dc, formattedText, road.PathGeometry);
        }
    }
    
    private bool ShouldRenderAtZoom(RoadImportance importance, double zoom)
    {
        return importance switch
        {
            RoadImportance.Interstate => zoom >= 0.25,
            RoadImportance.USHighway => zoom >= 0.5,
            RoadImportance.StateRoad => zoom >= 1.0,
            RoadImportance.LocalStreet => zoom >= 2.0,
            _ => false
        };
    }
}
```

**Data Source**:
- Extract road definitions from ATS game files using SCS Extractor
- Parse road geometry and names from `def/road/` and localization files
- Store in efficient spatial data structure (R-tree or quadtree) for fast lookup

### Phase 8: Multiplayer Support (TruckersMP)

**Features**:
- Show other players on the map
- Display player names and truck models
- Filter by friends/convoy members
- Real-time position updates via TruckersMP API

**Implementation**:
- Integrate TruckersMP API client
- Add player marker rendering for multiple trucks
- Implement efficient update mechanism for many players
- Add UI controls for filtering and player info



## Common Pitfalls and Troubleshooting

### Issue 1: Telemetry Plugin Not Loading

**Symptoms**:
- Application shows "Waiting for ATS to start..."
- No connection established even when ATS is running

**Diagnosis**:
1. Check `Documents\American Truck Simulator\game.log.txt`
2. Look for lines containing "plugin" or "telemetry"
3. Successful load shows: `[INFO] Loaded plugin: scs-telemetry.dll`

**Common Causes**:
- DLL not in correct folder (must be in `bin\win_x64\plugins\`)
- Wrong architecture (must be 64-bit DLL)
- Missing dependencies (Visual C++ Redistributable)
- ATS version too old (requires 1.49+)

**Solutions**:
```
1. Verify plugin path:
   Documents\American Truck Simulator\bin\win_x64\plugins\scs-telemetry.dll

2. Check DLL architecture:
   - Right-click DLL → Properties → Details
   - Should show "x64" or "64-bit"

3. Install VC++ Redistributable:
   - Download from Microsoft
   - Install both x86 and x64 versions

4. Update ATS:
   - Steam: Verify game files
   - Standalone: Download latest version
```

### Issue 2: Incorrect Coordinate Mapping

**Symptoms**:
- Player marker appears in wrong location
- Marker position is mirrored or rotated incorrectly
- Marker moves in opposite direction to truck

**Diagnosis**:
- Enable diagnostic logging
- Compare world coordinates to expected map position
- Check calibration point accuracy

**Common Causes**:
- Incorrect calibration points
- Swapped X/Z coordinates
- Wrong coordinate system orientation
- Map image doesn't match game version

**Solutions**:
```csharp
// Verify coordinate system
// ATS: X = East, Z = South (positive values)
// Map: X = Right, Y = Down

// Check transformation
var testPoint = new Vector3(0, 0, 0);
var mapPos = projection.WorldToMap(testPoint);
Console.WriteLine($"Origin maps to: {mapPos}");

// Verify calibration
foreach (var point in calibrationPoints)
{
    var calculated = projection.WorldToMap(point.WorldPosition);
    var error = Distance(calculated, point.MapPixelPosition);
    Console.WriteLine($"{point.Name}: Error = {error} pixels");
}
```

### Issue 3: Map Scaling Issues

**Symptoms**:
- Truck appears to move too fast or too slow on map
- Distance traveled doesn't match map movement
- Zoom levels feel wrong

**Diagnosis**:
- Measure known distance in-game (e.g., between two cities)
- Compare to map pixel distance
- Calculate scale factor

**Solutions**:
```csharp
// Recalibrate scale
// 1. Drive from City A to City B
// 2. Note world coordinates at both locations
// 3. Measure pixel distance on map
// 4. Calculate scale: pixels_per_meter = pixel_distance / world_distance

double worldDistance = startPos.Distance(endPos);
double pixelDistance = Math.Sqrt(
    Math.Pow(endPixel.X - startPixel.X, 2) +
    Math.Pow(endPixel.Y - startPixel.Y, 2)
);
double scale = pixelDistance / worldDistance;
Console.WriteLine($"Scale: {scale} pixels per meter");
```

### Issue 4: Connection Drops During Gameplay

**Symptoms**:
- Application loses connection randomly
- "Connection lost" errors appear
- Marker stops updating

**Common Causes**:
- Game paused or in menu
- Shared memory access conflict
- Insufficient polling interval
- Memory access exceptions

**Solutions**:
```csharp
// Implement robust reconnection
private async Task MonitorConnection()
{
    while (true)
    {
        try
        {
            if (!_client.IsConnected)
            {
                await _client.StartAsync();
            }
            await Task.Delay(5000);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Connection monitor: {ex.Message}");
        }
    }
}

// Add connection health check
private bool IsConnectionHealthy()
{
    return _client.IsConnected && 
           (DateTime.Now - _lastDataReceived).TotalSeconds < 5;
}
```

### Issue 5: Poor Performance / Low FPS

**Symptoms**:
- Map rendering is choppy
- High CPU usage
- Application feels sluggish

**Common Causes**:
- Rendering entire map every frame
- No image caching
- Blocking UI thread
- Inefficient coordinate calculations

**Solutions**:
```csharp
// Optimize rendering
protected override void OnRender(DrawingContext dc)
{
    // Cache map rendering
    if (_cachedMapVisual == null || _mapDirty)
    {
        _cachedMapVisual = RenderMapToVisual();
        _mapDirty = false;
    }
    dc.DrawDrawing(_cachedMapVisual);
    
    // Only render marker (changes frequently)
    RenderPlayerMarker(dc);
}

// Use async loading
private async Task LoadMapAsync()
{
    await Task.Run(() =>
    {
        var bitmap = LoadBitmapFromFile(_mapPath);
        
        Dispatcher.Invoke(() =>
        {
            MapImage = bitmap;
        });
    });
}

// Throttle updates
private DateTime _lastRender = DateTime.MinValue;
private const int MinFrameTimeMs = 33; // ~30 FPS

private void OnStateUpdated(PlayerState state)
{
    var now = DateTime.Now;
    if ((now - _lastRender).TotalMilliseconds < MinFrameTimeMs)
        return;
    
    _lastRender = now;
    InvalidateVisual();
}
```

### Issue 6: Memory Leaks

**Symptoms**:
- Memory usage grows over time
- Application becomes slower
- Eventually crashes with OutOfMemoryException

**Common Causes**:
- Event handlers not unsubscribed
- Bitmaps not disposed
- Timers not stopped
- Circular references

**Solutions**:
```csharp
// Implement proper disposal
public class MainViewModel : IDisposable
{
    public void Dispose()
    {
        _stateManager.StateUpdated -= OnStateUpdated;
        _telemetryClient.Dispose();
        _pollingTimer?.Dispose();
    }
}

// Use weak references for events
public class WeakEventManager
{
    private readonly List<WeakReference> _handlers = new();
    
    public void AddHandler(EventHandler handler)
    {
        _handlers.Add(new WeakReference(handler));
    }
    
    public void RaiseEvent(object sender, EventArgs e)
    {
        _handlers.RemoveAll(wr => !wr.IsAlive);
        
        foreach (var wr in _handlers)
        {
            if (wr.Target is EventHandler handler)
            {
                handler(sender, e);
            }
        }
    }
}
```

### Diagnostic Tools

**Enable Diagnostic Mode**:
```csharp
public class DiagnosticService
{
    private readonly StreamWriter _logWriter;
    
    public void LogTelemetry(TelemetryData data, Point mapPos)
    {
        var log = $"{DateTime.Now:HH:mm:ss.fff}," +
                  $"{data.Position.X:F2},{data.Position.Z:F2}," +
                  $"{mapPos.X:F0},{mapPos.Y:F0}," +
                  $"{data.Heading:F3},{data.Speed:F1}";
        
        _logWriter.WriteLine(log);
        _logWriter.Flush();
    }
}
```

**Performance Monitoring**:
```csharp
public class PerformanceMonitor
{
    private readonly Queue<long> _frameTimes = new();
    private readonly Stopwatch _stopwatch = new();
    
    public void BeginFrame()
    {
        _stopwatch.Restart();
    }
    
    public void EndFrame()
    {
        _stopwatch.Stop();
        _frameTimes.Enqueue(_stopwatch.ElapsedMilliseconds);
        
        if (_frameTimes.Count > 60)
            _frameTimes.Dequeue();
    }
    
    public double GetAverageFPS()
    {
        if (_frameTimes.Count == 0) return 0;
        double avgMs = _frameTimes.Average();
        return 1000.0 / avgMs;
    }
}
```



## Visual Design Guidelines

### Color Scheme

**Map Colors**:
- Background: `#F5F5DC` (Beige - represents terrain)
- Water: `#B0E0E6` (Powder Blue)
- Highways: `#FF6B35` (Orange-Red)
- Major Roads: `#FFD700` (Gold)
- Minor Roads: `#FFFFFF` (White)
- City Labels: `#2C3E50` (Dark Blue-Gray)
- Street Names: `#34495E` (Dark Gray-Blue)
- State Boundaries: `#95A5A6` (Gray)

**UI Colors**:
- Primary: `#3498DB` (Blue)
- Success: `#27AE60` (Green)
- Warning: `#F39C12` (Orange)
- Error: `#E74C3C` (Red)
- Background: `#ECF0F1` (Light Gray)
- Text: `#2C3E50` (Dark Gray)

**Player Marker**:
- Fill: `#E74C3C` (Red) with 80% opacity
- Outline: `#FFFFFF` (White) 2px stroke
- Shadow: `#000000` (Black) with 30% opacity, 2px offset

### Typography

**Map Labels**:
- City Names (Large): Segoe UI, 16pt, Bold
- City Names (Medium): Segoe UI, 12pt, Bold
- City Names (Small): Segoe UI, 10pt, Regular
- Street Names: Segoe UI, 9pt, Regular, follows road curve
- Road Numbers: Segoe UI, 10pt, Bold, White on colored background

**UI Text**:
- Headers: Segoe UI, 14pt, Semibold
- Body: Segoe UI, 11pt, Regular
- Status: Consolas, 10pt, Regular (monospace for alignment)

### Icon Design

**Player Marker Icon**:
```
    ▲
   ███
  █████
 ███████
   ███
   ███
```
- Triangle pointing in heading direction
- Approximately 24x24 pixels at 100% zoom
- Scales inversely with zoom level
- Drop shadow for depth

**UI Icons**:
- Zoom In: `+` symbol, 24x24px
- Zoom Out: `-` symbol, 24x24px
- Center on Player: Target/crosshair icon
- Settings: Gear icon
- Diagnostics: Chart/graph icon

### Layout

**Main Window**:
```
┌─────────────────────────────────────────────────────────┐
│ File  View  Tools  Help                    [_][□][X]    │ Menu Bar
├─────────────────────────────────────────────────────────┤
│ ┌─────┐ ┌─────┐ ┌─────┐                                │ Toolbar
│ │  +  │ │  -  │ │  ⊕  │  Zoom: 100%                    │
│ └─────┘ └─────┘ └─────┘                                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│                                                         │
│                    Map Canvas                           │
│                  (Interactive Area)                     │
│                                                         │
│                        ▲ Player                         │
│                                                         │
│                                                         │
├─────────────────────────────────────────────────────────┤
│ ● Connected | Position: (1234, 5678) | Speed: 55 MPH   │ Status Bar
└─────────────────────────────────────────────────────────┘
```

**Status Bar Sections**:
1. Connection indicator (colored dot + text)
2. Current position (world coordinates)
3. Current speed
4. FPS counter (diagnostic mode only)

### Animations

**Marker Movement**:
- Smooth interpolation between positions
- Easing function: Ease-out cubic
- Duration: 50ms per update

**Zoom Animation**:
- Smooth scale transition
- Duration: 200ms
- Easing: Ease-in-out

**Connection Status**:
- Pulsing animation when connecting
- Fade-in when connected
- Shake animation on error

### Accessibility

**Contrast Ratios**:
- Text on background: Minimum 4.5:1
- UI elements: Minimum 3:1
- Player marker: High contrast against all map colors

**Keyboard Navigation**:
- Tab: Cycle through controls
- Arrow keys: Pan map
- +/-: Zoom in/out
- Home: Center on player
- F11: Toggle full-screen

**Screen Reader Support**:
- Label all interactive elements
- Announce connection status changes
- Provide text alternatives for visual indicators



## Security Considerations

### Data Privacy

**Telemetry Data**:
- All telemetry data remains local to the user's machine
- No data is transmitted to external servers
- No analytics or tracking implemented

**File System Access**:
- Application only reads from:
  - Shared memory (telemetry)
  - Map asset files (read-only)
  - Configuration files (read-only)
- Application only writes to:
  - Log files (optional, user-controlled)
  - Configuration files (user settings)

### Memory Safety

**Shared Memory Access**:
```csharp
public class SafeMemoryReader
{
    public byte[] ReadMemory(MemoryMappedFile mmf, long offset, int length)
    {
        try
        {
            using (var accessor = mmf.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read))
            {
                byte[] buffer = new byte[length];
                accessor.ReadArray(0, buffer, 0, length);
                return buffer;
            }
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Error("Access denied to shared memory");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error($"Memory read error: {ex.Message}");
            throw;
        }
    }
}
```

### Input Validation

**Configuration Files**:
```csharp
public class ConfigurationValidator
{
    public bool ValidateConfiguration(AppConfiguration config)
    {
        // Validate zoom levels
        if (config.MinZoom <= 0 || config.MaxZoom <= config.MinZoom)
        {
            Logger.Warning("Invalid zoom configuration");
            return false;
        }
        
        // Validate file paths
        if (!string.IsNullOrEmpty(config.MapImagePath))
        {
            if (Path.IsPathRooted(config.MapImagePath) && 
                !config.MapImagePath.StartsWith(AppDomain.CurrentDomain.BaseDirectory))
            {
                Logger.Warning("Map path outside application directory");
                // Allow but log warning
            }
        }
        
        // Validate intervals
        if (config.TelemetryPollingIntervalMs < 10 || config.TelemetryPollingIntervalMs > 1000)
        {
            Logger.Warning("Polling interval out of reasonable range");
            return false;
        }
        
        return true;
    }
}
```

### Error Handling

**Graceful Degradation**:
- Application continues running if map fails to load (shows error message)
- Application continues running if telemetry connection fails (shows waiting state)
- Invalid configuration values fall back to safe defaults

**Exception Handling**:
```csharp
public class GlobalExceptionHandler
{
    public void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }
    
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Fatal($"Unhandled exception: {e.ExceptionObject}");
        
        MessageBox.Show(
            "A critical error occurred. The application will now close.\n\n" +
            "Please check the log file for details.",
            "Critical Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}
```

## Conclusion

This design document provides a comprehensive blueprint for building the ATS Live Map Desktop application. The architecture is modular, maintainable, and extensible, with clear separation of concerns between telemetry acquisition, data processing, and UI rendering.

Key design decisions:
- **C# with WPF** for familiar concepts and powerful graphics
- **scs-sdk-plugin** for telemetry without custom C++ development
- **Affine transformation** for accurate coordinate projection
- **Single large image** for MVP simplicity (tiling for future)
- **Linear interpolation** for smooth marker movement
- **Layered architecture** for maintainability and testability

The design addresses all 15 requirements from the requirements document and provides detailed implementation guidance, code examples, and troubleshooting information to support a web developer transitioning to desktop development.

Next steps: Proceed to the implementation plan (tasks.md) to break down the development into actionable coding tasks.

