# ATS Live Map - Telemetry Test Console

This is a simple console application to test and verify that telemetry data is being captured from American Truck Simulator.

## What It Tests

- ✅ Connection to ATS shared memory
- ✅ Telemetry data parsing (position, heading, speed)
- ✅ State manager functionality
- ✅ Position and heading smoothing
- ✅ Real-time data updates (~20 Hz)

## Prerequisites

1. **American Truck Simulator** must be installed
2. **Telemetry Plugin** must be installed in ATS:
   - Download the SCS Telemetry plugin
   - Copy the DLL to: `Documents\American Truck Simulator\bin\win_x64\plugins\`
   - The plugin creates a shared memory region that this app reads from

## How to Run

### Option 1: From Visual Studio
1. Right-click `ATSLiveMap.TestConsole` project
2. Select "Set as Startup Project"
3. Press F5 or click "Start"

### Option 2: From Command Line
```bash
# Build the project
dotnet build src/ATSLiveMap.TestConsole/ATSLiveMap.TestConsole.csproj

# Run the test console
dotnet run --project src/ATSLiveMap.TestConsole/ATSLiveMap.TestConsole.csproj
```

### Option 3: Run the Executable Directly
```bash
src\ATSLiveMap.TestConsole\bin\Debug\net8.0-windows\ATSLiveMap.TestConsole.exe
```

## Testing Steps

1. **Start the test console** (it will wait for ATS)
2. **Launch American Truck Simulator**
3. **Load a save game** (must be in-game, not in menu)
4. **Drive around** and watch the console update

## What You Should See

### When ATS is NOT running:
```
[TELEMETRY] Waiting for ATS to start...
```

### When ATS is running and connected:
```
===========================================
  ATS Live Map - Telemetry Test Console
===========================================

Status: CONNECTED
Game: American Truck Simulator
Paused: False
Updates received: 1234
Update rate: ~20.0 Hz

--- RAW TELEMETRY DATA ---
Position (World):
  X:    -12345.67 m
  Y:       123.45 m
  Z:     98765.43 m

Orientation:
  Heading:     1.5708 rad (  90.0°)
  Pitch:       0.0123 rad (   0.7°)
  Roll:       -0.0045 rad (  -0.3°)

Speed: 25.50 m/s (57.0 mph)
Timestamp: 14:23:45.123

--- STATE MANAGER (WITH SMOOTHING) ---
Map Position (Raw):
  X:      1234.56 px
  Y:      9876.54 px

Map Position (Smoothed):
  X:      1234.12 px
  Y:      9876.23 px

Heading (Raw):         1.5708 rad (  90.0°)
Heading (Smoothed):    1.5695 rad (  89.9°)

Speed: 25.50 m/s (57.0 mph)

Smoothing Delta:
  Position: 0.52 px
  Heading:  0.0013 rad (0.07°)

===========================================
Press Ctrl+C to exit
```

## What the Data Means

### Raw Telemetry Data
- **Position (World)**: Your truck's 3D coordinates in the game world (in meters)
- **Heading**: Direction you're facing (0° = North, 90° = East, 180° = South, 270° = West)
- **Pitch**: Up/down tilt (positive = nose up, negative = nose down)
- **Roll**: Left/right tilt (positive = rolling right, negative = rolling left)
- **Speed**: Current speed in m/s and mph

### State Manager (With Smoothing)
- **Map Position**: World coordinates converted to 2D map pixel coordinates
- **Smoothed values**: Filtered values to reduce jitter in the UI
- **Smoothing Delta**: How much the smoothing is adjusting the values

## Troubleshooting

### "Waiting for ATS to start..." forever
- Make sure ATS is actually running
- Verify the telemetry plugin is installed correctly
- Check `Documents\American Truck Simulator\game.log.txt` for plugin loading messages
- The plugin DLL should be in: `Documents\American Truck Simulator\bin\win_x64\plugins\`

### "Read error" or "JSON parse error"
- The plugin might not be compatible with your ATS version
- Try updating the telemetry plugin
- Check if the shared memory format has changed

### No position updates (all zeros)
- You must be in-game, not in the menu
- Load a save game and start driving
- The game must be actively running (not paused)

### Low update rate (< 10 Hz)
- This is normal if the game is paused or in menu
- Should be ~20 Hz when actively playing
- Check CPU usage - high CPU load can slow updates

## Exit

Press **Ctrl+C** to cleanly shut down the test console.

## Next Steps

Once you verify telemetry is working:
1. The full WPF UI application will use the same telemetry components
2. The map rendering will use the smoothed position values
3. The coordinate projection will convert world coordinates to map pixels using calibration points
