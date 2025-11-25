# ATS Live Map Desktop

A standalone desktop application that displays real-time truck position and heading on an interactive map for American Truck Simulator (ATS).

## Features

- Real-time position tracking from ATS telemetry
- Interactive map with pan and zoom controls
- Smooth player marker with heading indicator
- Automatic reconnection when ATS starts/stops
- High-resolution map display

## Prerequisites

- .NET 8.0 SDK or later
- American Truck Simulator (version 1.49+)
- scs-sdk-plugin telemetry DLL

## Setup

### 1. Install Telemetry Plugin

1. Download the latest `scs-telemetry.dll` from [scs-sdk-plugin releases](https://github.com/RenCloud/scs-sdk-plugin/releases)
2. Copy the DLL to: `Documents\American Truck Simulator\bin\win_x64\plugins\`
3. Create the `plugins` folder if it doesn't exist
4. Launch ATS to load the plugin
5. Verify in `Documents\American Truck Simulator\game.log.txt` for plugin initialization

### 2. Build the Application

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build ATSLiveMap.sln --configuration Release

# Run the application
dotnet run --project src/ATSLiveMap.UI/ATSLiveMap.UI.csproj

dotnet run --project
 src/ATSLiveMap.UI/ATSLiveMap.UI.csproj
```

## Project Structure

```
ATSLiveMap/
├── src/
│   ├── ATSLiveMap.Core/          # Core business logic
│   ├── ATSLiveMap.Telemetry/     # Telemetry layer
│   └── ATSLiveMap.UI/            # WPF application
├── assets/
│   ├── maps/                     # Map images
│   └── config/                   # Configuration files
└── ATSLiveMap.sln                # Solution file
```

## Technology Stack

- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Language**: C#
- **Packages**: System.Text.Json, MathNet.Numerics, Serilog

## Development

This project is designed to be developer-friendly for those coming from web development backgrounds. The architecture follows clear separation of concerns with three main layers:

1. **Telemetry Layer**: Reads game data via shared memory
2. **Application Layer**: Transforms data and manages state
3. **UI Layer**: Renders the map and handles user interaction

## Troubleshooting

### Plugin Not Loading
- Verify DLL is in correct folder: `Documents\American Truck Simulator\bin\win_x64\plugins\`
- Check `game.log.txt` for plugin loading errors
- Ensure ATS version is 1.49 or later

### Connection Issues
- Make sure ATS is running
- Check that the telemetry plugin loaded successfully
- Application will auto-reconnect when ATS starts

### Map Not Displaying
- Ensure map image is placed in `assets/maps/ats-map.png`
- Check file path in `appsettings.json`

## License

This project is for educational and personal use.

## Credits

- Telemetry plugin: [scs-sdk-plugin](https://github.com/RenCloud/scs-sdk-plugin)
- American Truck Simulator by SCS Software
