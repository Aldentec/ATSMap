# Implementation Plan

This implementation plan breaks down the ATS Live Map Desktop application into discrete, actionable coding tasks. Each task builds incrementally on previous work, with all code integrated into the main application. Tasks are organized to deliver a working MVP first, with optional enhancements marked with *.

## 🎓 Guide for Web Developers New to C# and Desktop Development

**Welcome!** This guide assumes you're coming from web development (JavaScript/TypeScript) and have no prior C# or desktop development experience. Don't worry - many concepts will feel familiar!

### Key Differences from Web Development

| Concept | Web Development | C# Desktop Development |
|---------|----------------|----------------------|
| **Language** | JavaScript/TypeScript (dynamic/optional typing) | C# (static typing, compiled) |
| **Runtime** | Browser/Node.js | .NET Runtime |
| **UI Framework** | React/Vue/Angular (HTML/CSS) | WPF (XAML, similar to HTML) |
| **Package Manager** | npm/yarn | NuGet |
| **Project Structure** | package.json, src/ folders | Solution (.sln), Projects (.csproj) |
| **Threading** | Single-threaded (event loop) | Multi-threaded (UI thread + background threads) |
| **Async** | Promises, async/await | Tasks, async/await (very similar!) |
| **Events** | EventEmitter, addEventListener | event keyword, EventHandler |
| **Styling** | CSS | XAML styles (similar to CSS) |

### C# Syntax Quick Reference for JS Developers

```csharp
// Variables (must declare type)
string name = "John";              // const name = "John"
int age = 25;                      // const age = 25
var auto = "inferred";             // let auto = "inferred" (type inferred)

// Functions/Methods
public int Add(int a, int b)       // function add(a, b) {
{                                  //   return a + b
    return a + b;                  // }
}

// Classes
public class Person                // class Person {
{                                  //   constructor(name) {
    public string Name { get; set; }//     this.name = name
                                   //   }
    public Person(string name)     // }
    {
        Name = name;
    }
}

// Async/Await (almost identical!)
public async Task<string> FetchData()  // async function fetchData() {
{                                      //   const response = await fetch(url)
    var result = await GetAsync();     //   return response
    return result;                     // }
}

// Null handling
string? nullable = null;           // let nullable: string | null = null
var value = nullable ?? "default"; // const value = nullable ?? "default"
var safe = nullable?.Length;       // const safe = nullable?.length
```

### How to Read These Tasks

Each task includes:
- **What you'll learn**: The new concept being introduced
- **Step-by-step**: Detailed instructions with code examples
- **C# concepts for web devs**: Translations to familiar web concepts
- **Why we do this**: Explanations of desktop-specific patterns

### Getting Help

- **Visual Studio IntelliSense**: Type a dot (.) after any object to see available methods (like autocomplete)
- **F12**: Go to definition of any class/method
- **Ctrl+.**: Quick actions and refactorings
- **Hover**: Hover over any code to see type information
- **Error List**: View all compilation errors (like TypeScript errors)

## Core Implementation Tasks

- [ ] 1. Set up project structure and development environment
  
  **What you'll learn**: How to create a C# solution (like a workspace), projects (like packages), and organize code in Visual Studio.
  
  **Step-by-step**:
  1. Open Visual Studio 2022
  2. Click "Create a new project"
  3. Search for "WPF Application" template, select it, click Next
  4. Name: `ATSLiveMap.UI`, Location: choose your workspace folder, click Next
  5. Framework: Select ".NET 8.0", click Create
  6. Right-click solution in Solution Explorer → Add → New Project
  7. Select "Class Library" template, name it `ATSLiveMap.Core`, click Create
  8. Repeat step 6-7 to create `ATSLiveMap.Telemetry` class library
  9. Right-click `ATSLiveMap.UI` → Add → Project Reference → Check both Core and Telemetry
  10. Right-click `ATSLiveMap.Telemetry` → Add → Project Reference → Check Core
  
  **Add NuGet packages** (like npm install):
  - Right-click `ATSLiveMap.Core` → Manage NuGet Packages → Browse tab
  - Search and install: `System.Text.Json`, `MathNet.Numerics`, `Serilog`
  - Repeat for `ATSLiveMap.Telemetry` (install `System.Text.Json`, `Serilog`)
  
  **Create folder structure** (like organizing src/ folders):
  - In `ATSLiveMap.Core`: Right-click project → Add → New Folder → Name it `Models`
  - Repeat to create folders: `Interfaces`, `Services`
  - In `ATSLiveMap.UI`: Create folders: `Views`, `ViewModels`, `Resources`
  
  **Create configuration files**:
  - Right-click `ATSLiveMap.UI` → Add → New Item → JSON File → Name: `appsettings.json`
  - Set "Copy to Output Directory" to "Copy if newer" in Properties window
  - Create another JSON file named `calibration.json` with same setting
  
  **C# concepts for web devs**:
  - Solution = Workspace/monorepo
  - Project = Package/module
  - Class Library = Shared library (like a utility package)
  - WPF Application = The executable app (like your main app entry point)
  - NuGet = npm/yarn
  
  _Requirements: 10.1, 10.2, 10.5, 12.1, 12.2_

- [x] 2. Implement core data models and interfaces





  
  **What you'll learn**: How to create C# classes (like TypeScript interfaces/classes), properties (like object properties), and structs (lightweight value types).
  
  - [x] 2.1 Create data model classes


    
    **Step-by-step**:
    1. Right-click `ATSLiveMap.Core/Models` folder → Add → Class → Name: `Vector3.cs`
    2. Replace the generated code with:
    ```csharp
    namespace ATSLiveMap.Core.Models
    {
        // struct is like a lightweight class for simple data (similar to a plain object in JS)
        public struct Vector3
        {
            // Properties are like object properties in JS, but with explicit types
            public float X { get; set; }  // East-West position
            public float Y { get; set; }  // Vertical (altitude)
            public float Z { get; set; }  // North-South position
            
            // Constructor - like a class constructor in JS
            public Vector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
            
            // Static property - like a static class property
            public static Vector3 Zero => new Vector3(0, 0, 0);
            
            // Method to calculate distance between two points
            public float Distance(Vector3 other)
            {
                float dx = X - other.X;
                float dy = Y - other.Y;
                float dz = Z - other.Z;
                return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }
    }
    ```
    
    3. Create `TelemetryData.cs` in Models folder:
    ```csharp
    using System;
    
    namespace ATSLiveMap.Core.Models
    {
        // class is like a class in JS/TS
        public class TelemetryData
        {
            public bool IsConnected { get; set; }
            public string GameName { get; set; } = string.Empty;  // = "" in JS
            public bool IsPaused { get; set; }
            public Vector3 Position { get; set; }
            public float Heading { get; set; }      // In radians
            public float Pitch { get; set; }
            public float Roll { get; set; }
            public float Speed { get; set; }        // In m/s
            public DateTime Timestamp { get; set; }  // Like Date in JS
        }
    }
    ```
    
    4. Create `PlayerState.cs`:
    ```csharp
    using System;
    using System.Windows;  // For Point type (X, Y coordinates)
    
    namespace ATSLiveMap.Core.Models
    {
        public class PlayerState
        {
            // Point is a WPF type for 2D coordinates (like {x, y} object in JS)
            public Point MapPosition { get; set; }
            public float Heading { get; set; }
            public float Speed { get; set; }
            public DateTime Timestamp { get; set; }
            
            // Smoothed values for rendering (reduces jitter)
            public Point SmoothedMapPosition { get; set; }
            public float SmoothedHeading { get; set; }
        }
    }
    ```
    
    5. Create `CalibrationPoint.cs`:
    ```csharp
    using System.Windows;
    
    namespace ATSLiveMap.Core.Models
    {
        public class CalibrationPoint
        {
            public string LocationName { get; set; } = string.Empty;
            public Vector3 WorldPosition { get; set; }      // ATS game coordinates
            public Point MapPixelPosition { get; set; }     // Pixel position on map image
        }
    }
    ```
    
    6. Create `AppConfiguration.cs`:
    ```csharp
    namespace ATSLiveMap.Core.Models
    {
        public class AppConfiguration
        {
            // Properties with default values (like default parameters in JS)
            public string MapImagePath { get; set; } = "assets/maps/ats-map.png";
            public int TelemetryPollingIntervalMs { get; set; } = 50;
            public int ReconnectIntervalMs { get; set; } = 2000;
            public float MinZoom { get; set; } = 0.25f;  // f suffix means float literal
            public float MaxZoom { get; set; } = 4.0f;
            public bool EnableDiagnostics { get; set; } = false;
            public string DiagnosticsLogPath { get; set; } = "logs/telemetry.log";
            
            // Smoothing parameters
            public float PositionSmoothingFactor { get; set; } = 0.3f;
            public float HeadingSmoothingFactor { get; set; } = 0.5f;
        }
    }
    ```
    
    **C# concepts for web devs**:
    - `struct` = Lightweight value type (passed by value, like primitives)
    - `class` = Reference type (passed by reference, like objects)
    - `{ get; set; }` = Auto-property (like a simple property in JS)
    - `public` = Accessible from anywhere (like export in JS)
    - `namespace` = Like a module/package name for organizing code
    - Type annotations are required (not optional like TypeScript)
    
    _Requirements: 1.2, 1.3, 5.1, 9.1_
  
  - [x] 2.2 Define service interfaces


    
    **What you'll learn**: Interfaces in C# (like TypeScript interfaces but for defining contracts that classes must implement).
    
    **Step-by-step**:
    1. Right-click `ATSLiveMap.Core/Interfaces` → Add → Class → Name: `ITelemetryClient.cs`
    2. Change `class` to `interface` and add methods:
    ```csharp
    using System;
    using System.Threading.Tasks;
    using ATSLiveMap.Core.Models;
    
    namespace ATSLiveMap.Core.Interfaces
    {
        // interface defines a contract (like TypeScript interface)
        // By convention, interface names start with 'I'
        public interface ITelemetryClient
        {
            // Properties (like interface properties in TS)
            bool IsConnected { get; }
            
            // Methods (like interface methods in TS)
            TelemetryData GetCurrentData();
            
            // Task<T> is like Promise<T> in JS
            Task<TelemetryData> GetCurrentDataAsync();
            
            // Events are like EventEmitter in Node.js
            event EventHandler<TelemetryData> DataUpdated;
            event EventHandler<string> ConnectionStatusChanged;
            
            // Async methods for starting/stopping
            Task StartAsync();
            Task StopAsync();
        }
    }
    ```
    
    3. Create `ICoordinateProjection.cs`:
    ```csharp
    using System.Collections.Generic;
    using System.Windows;
    using ATSLiveMap.Core.Models;
    
    namespace ATSLiveMap.Core.Interfaces
    {
        public interface ICoordinateProjection
        {
            // Convert 3D world position to 2D map pixel position
            Point WorldToMap(Vector3 worldPosition);
            
            // Convert 2D map pixel position back to 3D world position
            Vector3 MapToWorld(Point mapPosition);
            
            // Set up the transformation using known reference points
            void Calibrate(List<CalibrationPoint> referencePoints);
        }
    }
    ```
    
    4. Create `IMapService.cs`:
    ```csharp
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;  // For BitmapImage
    using ATSLiveMap.Core.Models;
    
    namespace ATSLiveMap.Core.Interfaces
    {
        public interface IMapService
        {
            // Load map image asynchronously (like async function in JS)
            Task<BitmapImage> LoadMapAsync();
            
            // Get map metadata (dimensions, reference points)
            MapMetadata GetMetadata();
        }
        
        // Helper class for map metadata
        public class MapMetadata
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public List<CalibrationPoint> ReferencePoints { get; set; } = new();
        }
    }
    ```
    
    5. Create `IStateManager.cs`:
    ```csharp
    using System;
    using ATSLiveMap.Core.Models;
    
    namespace ATSLiveMap.Core.Interfaces
    {
        public enum ConnectionStatus
        {
            Disconnected,
            Connecting,
            Connected,
            Error
        }
        
        public interface IStateManager
        {
            // Current player state
            PlayerState CurrentState { get; }
            
            // Connection status
            ConnectionStatus Status { get; }
            
            // Event fired when state updates (like EventEmitter)
            event EventHandler<PlayerState> StateUpdated;
            
            // Update state from new telemetry data
            void UpdateFromTelemetry(TelemetryData data);
        }
    }
    ```
    
    **C# concepts for web devs**:
    - `interface` = Contract that classes must implement (like TS interface for classes)
    - `Task<T>` = Promise<T> (represents async operation)
    - `async/await` = Same as JS async/await
    - `event` = Built-in pub/sub pattern (like EventEmitter)
    - `EventHandler<T>` = Event with typed data (like typed event listener)
    - `enum` = Enumeration of named constants (like TS enum)
    
    _Requirements: 9.4, 15.1_

- [x] 3. Implement telemetry layer





  
  **What you'll learn**: How to read shared memory (inter-process communication), implement async patterns, and handle events in C#.
  
  - [x] 3.1 Create shared memory telemetry client


    
    **What is shared memory?**: The telemetry plugin (DLL running inside ATS) writes game data to a shared memory region. Our app reads from that same memory region - like two apps sharing a file, but in RAM for speed.
    
    **Step-by-step**:
    1. Right-click `ATSLiveMap.Telemetry` → Add → Class → Name: `SharedMemoryTelemetryClient.cs`
    2. Implement the class:
    ```csharp
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;  // For shared memory access
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ATSLiveMap.Core.Interfaces;
    using ATSLiveMap.Core.Models;
    
    namespace ATSLiveMap.Telemetry
    {
        // Implements the interface we defined earlier
        public class SharedMemoryTelemetryClient : ITelemetryClient, IDisposable
        {
            // Constants (like const in JS)
            private const string MemoryMapName = "Local\\SCSTelemetry";
            private const int PollingIntervalMs = 50;  // 20 updates per second
            
            // Fields (like private class properties in JS)
            private MemoryMappedFile? _memoryMappedFile;  // ? means nullable
            private Timer? _pollingTimer;
            private TelemetryData _lastData = new();
            private ConnectionStatus _status = ConnectionStatus.Disconnected;
            private int _reconnectDelayMs = 1000;  // Start with 1 second
            
            // Properties
            public bool IsConnected => _status == ConnectionStatus.Connected;
            
            // Events (like EventEmitter.on('event', handler))
            public event EventHandler<TelemetryData>? DataUpdated;
            public event EventHandler<string>? ConnectionStatusChanged;
            
            // Synchronous method (not commonly used, but required by interface)
            public TelemetryData GetCurrentData()
            {
                return _lastData;
            }
            
            // Async method (like async function in JS)
            public async Task<TelemetryData> GetCurrentDataAsync()
            {
                // await Task.Run runs code on background thread
                return await Task.Run(() => _lastData);
            }
            
            // Start the telemetry client
            public async Task StartAsync()
            {
                // Infinite loop with retry logic
                while (true)
                {
                    try
                    {
                        _status = ConnectionStatus.Connecting;
                        OnConnectionStatusChanged("Connecting to ATS...");
                        
                        // Try to open the shared memory region
                        // This will throw FileNotFoundException if ATS isn't running
                        _memoryMappedFile = MemoryMappedFile.OpenExisting(MemoryMapName);
                        
                        _status = ConnectionStatus.Connected;
                        OnConnectionStatusChanged("Connected to ATS");
                        _reconnectDelayMs = 1000;  // Reset delay on success
                        
                        // Start polling timer (like setInterval in JS)
                        _pollingTimer = new Timer(
                            PollTelemetry,           // Callback function
                            null,                    // State (not used)
                            0,                       // Start immediately
                            PollingIntervalMs        // Repeat every 50ms
                        );
                        
                        break;  // Exit retry loop on success
                    }
                    catch (FileNotFoundException)
                    {
                        // ATS not running or plugin not loaded
                        _status = ConnectionStatus.Disconnected;
                        OnConnectionStatusChanged("Waiting for ATS to start...");
                        
                        // Wait before retrying (exponential backoff)
                        await Task.Delay(_reconnectDelayMs);
                        
                        // Increase delay for next retry (max 5 seconds)
                        _reconnectDelayMs = Math.Min(_reconnectDelayMs * 2, 5000);
                    }
                    catch (Exception ex)
                    {
                        _status = ConnectionStatus.Error;
                        OnConnectionStatusChanged($"Error: {ex.Message}");
                        await Task.Delay(_reconnectDelayMs);
                    }
                }
            }
            
            // Stop the telemetry client
            public Task StopAsync()
            {
                _pollingTimer?.Dispose();
                _memoryMappedFile?.Dispose();
                return Task.CompletedTask;
            }
            
            // Called by timer every 50ms
            private void PollTelemetry(object? state)
            {
                // Implementation in next sub-task
            }
            
            // Helper to raise events (like emit in EventEmitter)
            protected virtual void OnDataUpdated(TelemetryData data)
            {
                DataUpdated?.Invoke(this, data);  // ?. is null-conditional operator
            }
            
            protected virtual void OnConnectionStatusChanged(string message)
            {
                ConnectionStatusChanged?.Invoke(this, message);
            }
            
            // Cleanup (like componentWillUnmount in React)
            public void Dispose()
            {
                _pollingTimer?.Dispose();
                _memoryMappedFile?.Dispose();
            }
        }
    }
    ```
    
    **C# concepts for web devs**:
    - `MemoryMappedFile` = Shared memory access (like SharedArrayBuffer in JS, but easier)
    - `Timer` = setInterval (but on background thread)
    - `Task.Delay` = setTimeout as Promise
    - `IDisposable` = Interface for cleanup (like componentWillUnmount)
    - `?.` = Optional chaining (same as JS)
    - `??` = Nullish coalescing (same as JS)
    - `private` = Private field (like # in JS classes)
    
    _Requirements: 1.1, 1.2, 1.4, 1.5, 2.1_
  

  - [x] 3.2 Implement telemetry data parsing

    
    **What you'll learn**: JSON parsing in C# (like JSON.parse), error handling with try-catch, and data validation.
    
    **Step-by-step**:
    1. Complete the `PollTelemetry` method in `SharedMemoryTelemetryClient.cs`:
    ```csharp
    using System.Text.Json;  // Add this using at top
    
    private void PollTelemetry(object? state)
    {
        try
        {
            if (_memoryMappedFile == null) return;
            
            // Create a view accessor to read from shared memory
            using (var accessor = _memoryMappedFile.CreateViewAccessor(
                0,                              // Start at beginning
                0,                              // Read entire region
                MemoryMappedFileAccess.Read))   // Read-only access
            {
                // Read bytes from memory
                byte[] buffer = new byte[accessor.Capacity];
                accessor.ReadArray(0, buffer, 0, buffer.Length);
                
                // Convert bytes to string (like Buffer.toString() in Node.js)
                string json = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                
                // Parse JSON (like JSON.parse())
                var data = ParseTelemetryJson(json);
                
                if (data != null)
                {
                    _lastData = data;
                    OnDataUpdated(data);
                }
            }
        }
        catch (Exception ex)
        {
            _status = ConnectionStatus.Error;
            OnConnectionStatusChanged($"Read error: {ex.Message}");
            
            // Attempt reconnection
            _pollingTimer?.Dispose();
            _ = StartAsync();  // Fire and forget (like not awaiting a promise)
        }
    }
    
    private TelemetryData? ParseTelemetryJson(string json)
    {
        try
        {
            // JsonDocument is like JSON.parse() but more efficient
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            
            // Navigate JSON structure (like accessing obj.truck.position.X)
            var truck = root.GetProperty("truck");
            var position = truck.GetProperty("position");
            var orientation = truck.GetProperty("orientation");
            var game = root.GetProperty("game");
            
            // Create and populate TelemetryData object
            return new TelemetryData
            {
                IsConnected = game.GetProperty("connected").GetBoolean(),
                GameName = game.GetProperty("gameName").GetString() ?? "",
                IsPaused = game.GetProperty("paused").GetBoolean(),
                
                Position = new Vector3(
                    position.GetProperty("X").GetSingle(),  // GetSingle = get float
                    position.GetProperty("Y").GetSingle(),
                    position.GetProperty("Z").GetSingle()
                ),
                
                Heading = orientation.GetProperty("heading").GetSingle(),
                Pitch = orientation.GetProperty("pitch").GetSingle(),
                Roll = orientation.GetProperty("roll").GetSingle(),
                Speed = truck.GetProperty("speed").GetSingle(),
                
                Timestamp = DateTime.Now
            };
        }
        catch (JsonException ex)
        {
            // JSON parsing failed (malformed data)
            Console.WriteLine($"JSON parse error: {ex.Message}");
            return null;
        }
        catch (KeyNotFoundException ex)
        {
            // Expected property not found in JSON
            Console.WriteLine($"Missing JSON property: {ex.Message}");
            return null;
        }
    }
    ```
    
    **C# concepts for web devs**:
    - `using` statement = Automatic cleanup (like try-finally or RAII)
    - `Encoding.UTF8.GetString()` = Buffer.toString('utf8')
    - `JsonDocument` = Efficient JSON parser (like JSON.parse but lower-level)
    - `GetProperty()` = Access JSON property (like obj.prop)
    - `GetSingle()` = Get float value (like parseFloat)
    - `??` = Nullish coalescing operator (same as JS)
    
    _Requirements: 1.2, 1.3, 14.1_
  
  - [x] 3.3 Create telemetry polling service


    
    **What you'll learn**: Background threading, marshaling to UI thread (important for desktop apps), and cancellation tokens.
    
    **Why do we need this?**: In desktop apps, only the UI thread can update UI elements. Background threads must "marshal" (transfer) data to the UI thread. This is different from web dev where you can update DOM from anywhere.
    
    **Step-by-step**:
    1. Right-click `ATSLiveMap.Telemetry` → Add → Class → Name: `TelemetryPoller.cs`
    2. Implement the poller:
    ```csharp
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;  // For Application.Current.Dispatcher
    using ATSLiveMap.Core.Interfaces;
    
    namespace ATSLiveMap.Telemetry
    {
        public class TelemetryPoller
        {
            private readonly ITelemetryClient _client;
            private readonly IStateManager _stateManager;
            private CancellationTokenSource? _cts;  // For canceling async operations
            
            // Constructor (like constructor in JS class)
            public TelemetryPoller(ITelemetryClient client, IStateManager stateManager)
            {
                _client = client;
                _stateManager = stateManager;
            }
            
            public async Task StartAsync()
            {
                // Create cancellation token (like AbortController in fetch API)
                _cts = new CancellationTokenSource();
                
                // Run on background thread (like Web Worker)
                await Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // Get telemetry data on background thread
                            var data = await _client.GetCurrentDataAsync();
                            
                            // Marshal to UI thread (IMPORTANT!)
                            // This is like postMessage to main thread in Web Workers
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                // This code runs on UI thread
                                _stateManager.UpdateFromTelemetry(data);
                            });
                            
                            // Wait 50ms before next poll (20 Hz)
                            await Task.Delay(50, _cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // Normal cancellation, exit loop
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Polling error: {ex.Message}");
                            // Continue polling despite errors
                        }
                    }
                }, _cts.Token);
            }
            
            public void Stop()
            {
                // Cancel the polling loop (like AbortController.abort())
                _cts?.Cancel();
                _cts?.Dispose();
            }
        }
    }
    ```
    
    **C# concepts for web devs**:
    - `Task.Run` = Run code on background thread (like Web Worker)
    - `Dispatcher.InvokeAsync` = Marshal to UI thread (like postMessage from worker)
    - `CancellationToken` = AbortController/AbortSignal
    - Desktop apps have thread affinity for UI (web doesn't have this restriction)
    - Background threads cannot touch UI elements directly
    
    _Requirements: 13.1, 1.2_

- [x] 4. Implement coordinate projection system





  - [x] 4.1 Create affine transformation implementation


    - Implement AffineCoordinateProjection class with transformation matrix
    - Write Calibrate method using least-squares solution for affine parameters
    - Implement WorldToMap method applying transformation: [x_map, y_map] = M * [x_world, z_world] + [tx, ty]
    - Add coordinate validation and clamping to map bounds
    - _Requirements: 5.1, 5.2, 5.3, 5.4_
  

  - [x] 4.2 Load and apply calibration points

    - Read calibration.json file with reference points (Los Angeles, San Francisco, Las Vegas)
    - Parse calibration data into CalibrationPoint objects
    - Call Calibrate method with loaded points on application startup
    - Log calibration accuracy (error in pixels for each reference point)
    - _Requirements: 5.2, 5.5, 14.4_


- [x] 5. Implement map service and loading






  - [x] 5.1 Create single-image map service

    - Implement SingleImageMapService class that loads PNG map image
    - Use BitmapImage with CacheOption.OnLoad for efficient loading
    - Freeze bitmap to make it thread-safe and immutable
    - Cache loaded image to avoid repeated disk reads
    - _Requirements: 4.1, 4.4, 13.2_
  
  - [x] 5.2 Implement map metadata management


    - Create MapMetadata class with width, height, and reference points
    - Load metadata from configuration file
    - Provide metadata to coordinate projection system
    - Validate map file exists and is readable before loading
    - _Requirements: 4.5, 14.3_
  
  - [x] 5.3 Add error handling for map loading


    - Handle FileNotFoundException with user-friendly error message
    - Display error dialog if map fails to load with file path information
    - Allow application to continue running without map (show error state)
    - Log all map loading errors to diagnostic log
    - _Requirements: 14.3, 14.5_

- [x] 6. Implement state management and smoothing






  - [x] 6.1 Create state manager service

    - Implement StateManager class that receives telemetry updates
    - Apply coordinate projection to convert world position to map position
    - Maintain current PlayerState with both raw and smoothed values
    - Emit StateUpdated events when player state changes
    - _Requirements: 9.2, 7.3, 1.2_
  
  - [x] 6.2 Implement position smoothing


    - Create LinearSmoother class with configurable smoothing factor (default 0.3)
    - Apply linear interpolation to position: smoothed = last + (new - last) * factor
    - Apply smoothing to heading angle with separate factor (default 0.5)
    - Ensure smoothing doesn't introduce lag exceeding 500ms
    - _Requirements: 8.1, 8.2, 8.4, 8.5_
  
  - [x] 6.3 Add connection status management


    - Track connection status (Disconnected, Connecting, Connected, Error)
    - Emit connection status change events
    - Display appropriate status messages for each state
    - Implement automatic reconnection when connection is lost
    - _Requirements: 1.4, 1.5, 14.1_

- [x] 7. Create WPF UI foundation




  - [x] 7.1 Set up main window and MVVM structure


    - Create MainWindow.xaml with menu bar, toolbar, canvas area, and status bar
    - Implement MainViewModel with INotifyPropertyChanged
    - Set up data binding between ViewModel and View
    - Add dependency injection container for service resolution
    - _Requirements: 3.3, 3.5, 9.3_
  
  - [x] 7.2 Implement view model properties and commands


    - Add properties: CurrentPlayerState, ConnectionStatus, IsConnected, ZoomLevel
    - Implement ICommand for ZoomIn, ZoomOut, CenterOnPlayer
    - Wire up property change notifications
    - Bind ViewModel properties to UI elements
    - _Requirements: 6.1, 6.2, 6.3, 9.3_
  
  - [x] 7.3 Create status bar with connection indicator


    - Display connection status with colored indicator (green=connected, red=disconnected, yellow=connecting)
    - Show current world coordinates in status bar
    - Display current speed in MPH
    - Add FPS counter for diagnostic mode
    - _Requirements: 1.4, 1.5, 14.4_


- [x] 8. Implement map canvas rendering





  - [x] 8.1 Create custom MapCanvas control


    - Create MapCanvas class inheriting from FrameworkElement
    - Define dependency properties: MapImage, PlayerPosition, PlayerHeading, ZoomLevel
    - Override OnRender method to draw map and player marker
    - Set up TransformGroup with ScaleTransform and TranslateTransform for zoom/pan
    - _Requirements: 4.1, 6.5, 7.1, 9.3_
  
  - [x] 8.2 Implement map image rendering


    - Draw map image using DrawingContext.DrawImage in OnRender
    - Apply scale and translate transforms for zoom and pan
    - Implement dirty region tracking to avoid unnecessary re-renders
    - Cache map rendering when only player marker changes
    - _Requirements: 4.1, 6.5, 13.1_
  
  - [x] 8.3 Implement player marker rendering


    - Create triangle geometry pointing upward (top point at 0, -20)
    - Apply rotation transform based on heading angle (convert radians to degrees)
    - Apply translation transform to position marker at player location
    - Draw marker with red fill and white outline (2px stroke)
    - Add yellow dot at exact position for precision
    - _Requirements: 7.1, 7.2, 7.4, 7.5_
  

  - [x] 8.4 Add marker scaling with zoom level

    - Scale marker inversely with zoom to maintain constant screen size
    - Clamp marker scale between 0.5 and 2.0
    - Ensure marker remains visible and distinguishable at all zoom levels
    - Apply scale transform before rotation and translation
    - _Requirements: 7.5, 6.4_

- [x] 9. Implement interactive map controls






  - [x] 9.1 Add mouse pan functionality

    - Handle MouseDown event to start panning (capture mouse)
    - Handle MouseMove event to update translate transform based on drag delta
    - Handle MouseUp event to stop panning (release mouse capture)
    - Update last mouse position on each move for smooth panning
    - _Requirements: 6.1, 6.5_
  

  - [x] 9.2 Add mouse wheel zoom functionality

    - Handle MouseWheel event to detect scroll direction
    - Calculate zoom factor: 1.1 for zoom in, 0.9 for zoom out
    - Clamp zoom level between MinZoom (0.25) and MaxZoom (4.0)
    - Adjust translate transform to zoom toward mouse cursor position
    - _Requirements: 6.2, 6.3, 6.4, 6.5_
  

  - [x] 9.3 Implement center-on-player command

    - Calculate translation needed to center player in viewport
    - Set translate transform to center player marker
    - Trigger re-render after centering
    - Bind to toolbar button and keyboard shortcut (Home key)
    - _Requirements: 6.1, 6.5_
  

  - [x] 9.4 Add zoom in/out toolbar buttons

    - Create toolbar with + and - buttons
    - Implement zoom commands that adjust scale transform
    - Zoom toward center of viewport when using buttons
    - Display current zoom percentage in toolbar
    - _Requirements: 6.2, 6.3, 6.4_

- [x] 10. Implement real-time updates and rendering loop





  - [x] 10.1 Wire up telemetry to state manager


    - Subscribe StateManager to TelemetryClient.DataUpdated event
    - Call StateManager.UpdateFromTelemetry on each telemetry update
    - Ensure updates happen on background thread, marshal to UI thread for rendering
    - Add error handling for update processing
    - _Requirements: 1.2, 7.3, 8.3, 13.1_
  
  - [x] 10.2 Connect state manager to UI


    - Subscribe MainViewModel to StateManager.StateUpdated event
    - Update CurrentPlayerState property when state changes
    - Trigger MapCanvas.InvalidateVisual to re-render
    - Implement frame rate limiting to maintain 30+ FPS
    - _Requirements: 7.3, 8.3, 6.5_
  
  - [x] 10.3 Optimize rendering performance


    - Implement dirty region tracking (map vs marker)
    - Only re-render map when zoom/pan changes
    - Always re-render marker (updates frequently)
    - Add frame time measurement for performance monitoring
    - _Requirements: 6.5, 13.1, 13.4_


- [ ] 11. Add configuration and settings management
  - [ ] 11.1 Implement configuration loading
    - Read appsettings.json on application startup
    - Deserialize into AppConfiguration object
    - Validate configuration values (zoom ranges, intervals, paths)
    - Apply default values for missing or invalid settings
    - _Requirements: 12.5, 14.2_
  
  - [ ] 11.2 Create settings UI dialog
    - Create SettingsWindow.xaml with configuration options
    - Add controls for: map path, polling interval, smoothing factors, zoom limits
    - Implement save/cancel functionality
    - Persist changes back to appsettings.json
    - _Requirements: 8.5, 12.5_
  
  - [ ] 11.3 Add map asset path configuration
    - Allow user to browse for map image file
    - Validate file exists and is a valid image format
    - Update configuration and reload map when path changes
    - Display current map file name in settings
    - _Requirements: 4.5, 12.5, 14.3_

- [ ] 12. Implement error handling and diagnostics
  - [ ] 12.1 Add global exception handling
    - Set up AppDomain.UnhandledException handler
    - Set up Application.DispatcherUnhandledException handler
    - Set up TaskScheduler.UnobservedTaskException handler
    - Display user-friendly error dialog on critical errors
    - _Requirements: 14.1, 14.2, 14.3_
  
  - [ ] 12.2 Implement diagnostic logging
    - Configure Serilog with file sink to logs/telemetry.log
    - Log connection status changes
    - Log coordinate transformation errors
    - Log performance metrics (FPS, update latency)
    - _Requirements: 14.4, 14.5_
  
  - [ ] 12.3 Create diagnostic mode UI
    - Add menu option to toggle diagnostic mode
    - Display FPS counter in status bar when enabled
    - Show raw telemetry values in overlay panel
    - Display coordinate transformation details
    - _Requirements: 14.4, 14.5_
  
  - [ ] 12.4 Add user-friendly error messages
    - Create ErrorMessageProvider with context-specific messages
    - Handle FileNotFoundException (plugin not running)
    - Handle UnauthorizedAccessException (permission denied)
    - Handle JsonException (invalid telemetry format)
    - _Requirements: 14.1, 14.2, 14.3, 14.5_

- [ ] 13. Create application startup and initialization
  - [ ] 13.1 Implement application bootstrap
    - Create App.xaml.cs with OnStartup override
    - Initialize dependency injection container
    - Register all services (telemetry, map, state, projection)
    - Load configuration and calibration data
    - _Requirements: 10.5, 12.4_
  
  - [ ] 13.2 Add service lifecycle management
    - Start TelemetryPoller on application startup
    - Implement IDisposable for all services
    - Stop all services and dispose resources on application shutdown
    - Handle cleanup in App.OnExit
    - _Requirements: 13.1, 1.1_
  
  - [ ] 13.3 Create splash screen or loading indicator
    - Display loading screen while initializing services
    - Show progress for map loading
    - Display "Waiting for ATS..." message if game not running
    - Transition to main window when ready
    - _Requirements: 1.5, 4.5_


- [ ] 14. Polish UI and visual design
  - [ ] 14.1 Apply color scheme and styling
    - Create AppStyles.xaml with color definitions
    - Apply colors: Primary (#3498DB), Success (#27AE60), Warning (#F39C12), Error (#E74C3C)
    - Style buttons, menus, and status bar with consistent theme
    - Set window background to #ECF0F1
    - _Requirements: 3.5, 7.4_
  
  - [ ] 14.2 Add application icon and branding
    - Create or source truck/map icon for application
    - Set window icon and taskbar icon
    - Add application title and version to window title bar
    - Create about dialog with application information
    - _Requirements: 3.5_
  
  - [ ] 14.3 Implement keyboard shortcuts
    - Add keyboard shortcuts: Arrow keys for pan, +/- for zoom, Home for center
    - Add F11 for full-screen toggle
    - Add Ctrl+S for settings dialog
    - Display keyboard shortcuts in help menu
    - _Requirements: 6.1, 6.2, 6.3_
  
  - [ ] 14.4 Add tooltips and help text
    - Add tooltips to all toolbar buttons
    - Add status bar help text that changes based on mouse hover
    - Create help menu with user guide link
    - Add context-sensitive help for error states
    - _Requirements: 14.5_

- [ ] 15. Create documentation and setup guide
  - [ ] 15.1 Write user setup guide
    - Document telemetry plugin installation steps
    - Provide screenshots of plugin folder location
    - Explain how to verify plugin is loaded in game.log.txt
    - Include troubleshooting section for common setup issues
    - _Requirements: 2.2, 2.3, 2.4, 10.1, 10.2_
  
  - [ ] 15.2 Create developer documentation
    - Document project structure and architecture
    - Explain coordinate projection system with examples
    - Provide API documentation for key interfaces
    - Include build and deployment instructions
    - _Requirements: 5.4, 5.5, 10.3, 10.4, 12.3_
  
  - [ ] 15.3 Write troubleshooting guide
    - Document common issues: plugin not loading, incorrect coordinates, connection drops
    - Provide diagnostic steps for each issue
    - Include solutions and workarounds
    - Add FAQ section
    - _Requirements: 14.5, 2.4_

## Optional Enhancement Tasks

- [ ]* 16. Write unit tests for core functionality
  - [ ]* 16.1 Test coordinate projection
    - Write tests for WorldToMap with known calibration points
    - Test coordinate clamping for out-of-bounds positions
    - Test calibration accuracy with multiple reference points
    - _Requirements: 5.3_
  
  - [ ]* 16.2 Test telemetry data parsing
    - Write tests for JSON deserialization with valid data
    - Test error handling for malformed JSON
    - Test data validation for out-of-range values
    - _Requirements: 1.2, 1.3_
  
  - [ ]* 16.3 Test position smoothing
    - Write tests for linear interpolation algorithm
    - Test smoothing with jittery input data
    - Verify smoothing doesn't introduce excessive lag
    - _Requirements: 8.1, 8.2, 8.4_

- [ ]* 17. Implement tiled map system (future enhancement)
  - [ ]* 17.1 Create tile loading service
    - Implement tile-based map service with zoom levels
    - Load only visible tiles plus margin
    - Cache loaded tiles in memory
    - _Requirements: 4.4, 13.3_
  
  - [ ]* 17.2 Generate map tiles from source image
    - Create tile generation tool that splits large image into tiles
    - Generate multiple zoom levels (1:1, 1:2, 1:4, etc.)
    - Save tiles in organized folder structure
    - _Requirements: 4.4, 4.5_

- [ ]* 18. Add advanced smoothing with Kalman filter
  - [ ]* 18.1 Implement Kalman filter smoother
    - Create KalmanSmoother class with prediction and update steps
    - Configure process noise and measurement noise parameters
    - Apply to position and heading separately
    - _Requirements: 8.1, 8.5_
  
  - [ ]* 18.2 Compare smoothing algorithms
    - Add configuration option to choose smoothing algorithm
    - Measure and compare latency and smoothness
    - Document trade-offs between algorithms
    - _Requirements: 8.5_


- [ ]* 19. Add job information display (Phase 2 enhancement)
  - [ ]* 19.1 Extend telemetry data model for job info
    - Add job fields to TelemetryData: cargo type, weight, source, destination, deadline
    - Parse job information from telemetry JSON
    - Update StateManager to track job state
    - _Requirements: 15.2, 15.3_
  
  - [ ]* 19.2 Create job info panel UI
    - Create JobInfoPanel user control with job details
    - Display cargo, weight, route, distance, ETA, revenue
    - Position panel as overlay on main window
    - Add show/hide toggle for job panel
    - _Requirements: 15.2, 15.4_

- [ ]* 20. Add truck statistics HUD (Phase 3 enhancement)
  - [ ]* 20.1 Extend telemetry for truck stats
    - Add truck fields: RPM, fuel level, damage, gear
    - Parse truck statistics from telemetry
    - Update StateManager with truck state
    - _Requirements: 15.1, 15.2_
  
  - [ ]* 20.2 Create truck stats panel UI
    - Create TruckStatsPanel with gauges and indicators
    - Display speed, RPM, fuel, damage, gear
    - Use circular gauges for RPM and speed
    - Add warning indicators for low fuel and damage
    - _Requirements: 15.2, 15.4_

- [ ]* 21. Implement route visualization (Phase 4 enhancement)
  - [ ]* 21.1 Parse navigation waypoints from telemetry
    - Extend telemetry to include navigation data
    - Parse waypoint coordinates and route information
    - Store route as list of Vector3 positions
    - _Requirements: 15.1, 15.2_
  
  - [ ]* 21.2 Render route on map
    - Create RouteRenderer class
    - Convert waypoints to map coordinates
    - Draw polyline connecting waypoints
    - Highlight current road segment
    - _Requirements: 15.2, 15.4_

- [ ]* 22. Add explored area tracking (Phase 5 enhancement)
  - [ ]* 22.1 Implement exploration tracking
    - Create ExplorationTracker with grid-based tracking
    - Mark cells as explored when player passes through
    - Calculate exploration percentage
    - Persist exploration data to file
    - _Requirements: 15.1, 15.2, 15.5_
  
  - [ ]* 22.2 Render fog-of-war overlay
    - Create semi-transparent overlay for unexplored areas
    - Update overlay as player explores new regions
    - Add toggle to show/hide exploration overlay
    - Display exploration statistics
    - _Requirements: 15.2, 15.4_

## Notes

- All core tasks (1-15) must be completed for a functional MVP
- Optional tasks (16-22) marked with * can be implemented later
- Each task includes specific requirements references for traceability
- Tasks are designed to be implemented sequentially within each major section
- Testing tasks are optional to focus on core functionality first
- Future enhancement tasks (17-22) correspond to design phases 2-5

