# Requirements Document

## Introduction

This document specifies the requirements for a standalone desktop application that displays a live, interactive map for American Truck Simulator (ATS). The application will read real-time telemetry data from ATS and render the player's position and heading on a high-resolution, zoomable map. The target user is a web developer with JavaScript/TypeScript experience but no prior knowledge of ATS modding, telemetry systems, or the SCS SDK.

## Glossary

- **ATS**: American Truck Simulator, a truck simulation game developed by SCS Software
- **Telemetry SDK**: The official SCS Software Telemetry & Input SDK that allows external applications to read game state data
- **Telemetry Plugin**: A DLL (Dynamic Link Library) that ATS loads to expose game data to external applications
- **Desktop Application**: A native application that runs on Windows as a standalone executable (not a web browser application)
- **Player Marker**: A visual indicator on the map showing the truck's current position and heading
- **World Coordinates**: The internal coordinate system used by ATS to represent positions in the game world
- **Map Projection**: The mathematical transformation that converts ATS world coordinates to pixel coordinates on the displayed map
- **Bridge Service**: A software component that transfers telemetry data from the game plugin to the desktop application
- **Shared Memory**: A memory region accessible by multiple processes, commonly used for inter-process communication
- **Map Tiles**: Small image segments that together form a complete map, loaded on-demand for performance
- **HUD**: Heads-Up Display, on-screen information overlays showing game statistics

## Requirements

### Requirement 1: Telemetry Data Acquisition

**User Story:** As a user, I want the application to automatically read my truck's position and heading from ATS while I play, so that I can see my location on the map without manual input.

#### Acceptance Criteria

1. WHEN the Desktop Application starts, THE Desktop Application SHALL establish a connection to the Telemetry Plugin
2. WHILE ATS is running with the Telemetry Plugin loaded, THE Desktop Application SHALL read the player's X, Y, and Z world coordinates at a minimum frequency of 10 updates per second
3. WHILE ATS is running with the Telemetry Plugin loaded, THE Desktop Application SHALL read the player's heading angle in degrees or radians
4. IF the Telemetry Plugin connection fails or is lost, THEN THE Desktop Application SHALL display a clear error message indicating the connection status
5. WHEN ATS is not running, THE Desktop Application SHALL display a waiting state and attempt to reconnect at intervals not exceeding 5 seconds

### Requirement 2: Telemetry Plugin Integration

**User Story:** As a beginner with no ATS modding experience, I want clear guidance on setting up the telemetry plugin, so that I can get data from the game without extensive technical knowledge.

#### Acceptance Criteria

1. THE Desktop Application SHALL support integration with at least one community-maintained telemetry plugin that requires no custom C++ development
2. THE Desktop Application documentation SHALL include step-by-step installation instructions for the chosen telemetry plugin
3. THE Desktop Application documentation SHALL specify the exact file paths where the telemetry plugin DLL must be placed
4. THE Desktop Application documentation SHALL explain how to verify that ATS has successfully loaded the telemetry plugin
5. WHERE the user chooses to build a custom telemetry plugin, THE Desktop Application documentation SHALL provide a complete guide including SDK setup, compilation steps, and deployment instructions

### Requirement 3: Desktop Application Framework

**User Story:** As a web developer learning desktop development, I want the application built with a framework that leverages my existing JavaScript/TypeScript knowledge or has a gentle learning curve, so that I can understand and modify the code.

#### Acceptance Criteria

1. THE Desktop Application SHALL be built using one of the following technology stacks: C# with WPF, C# with .NET MAUI, Electron packaged as desktop executable, or Tauri packaged as desktop executable
2. THE Desktop Application documentation SHALL justify the chosen technology stack with specific reasons relevant to a web developer's background
3. THE Desktop Application SHALL run as a native Windows executable file
4. THE Desktop Application SHALL NOT require a web browser to display the user interface
5. THE Desktop Application SHALL provide a main window with standard window controls including minimize, maximize, and close buttons

### Requirement 4: Map Rendering and Display

**User Story:** As a user, I want to see a beautiful, detailed map of the ATS world with cities, roads, and labels, so that I can understand my location in the game world at a glance.

#### Acceptance Criteria

1. THE Desktop Application SHALL display a map of the ATS game world with a minimum resolution of 4096x4096 pixels
2. THE Desktop Application SHALL render visible city names on the map
3. THE Desktop Application SHALL render major highway routes on the map
4. THE Desktop Application SHALL display street names for major roads and highways on the map
5. THE Desktop Application SHALL support either a single large map image or a tiled map system for performance optimization
6. THE Desktop Application documentation SHALL explain how to obtain or generate the map imagery from ATS game files or community resources

### Requirement 5: Coordinate Projection System

**User Story:** As a user, I want my truck to appear in the correct location on the map, so that the map accurately reflects my in-game position.

#### Acceptance Criteria

1. THE Desktop Application SHALL implement a Map Projection function that converts ATS World Coordinates to pixel coordinates on the displayed map
2. THE Desktop Application SHALL calibrate the Map Projection using at least three known reference points (city locations with both world coordinates and map pixel positions)
3. WHEN the player's truck is at a known city location in ATS, THE Desktop Application SHALL display the Player Marker within 50 pixels of the correct map position at 100% zoom level
4. THE Desktop Application documentation SHALL provide the mathematical formulas used for coordinate transformation
5. THE Desktop Application documentation SHALL include worked examples showing the conversion of sample coordinates

### Requirement 6: Interactive Map Controls

**User Story:** As a user, I want to pan and zoom the map smoothly using my mouse, so that I can explore different areas and zoom levels like I would in Google Maps.

#### Acceptance Criteria

1. WHEN the user drags the mouse while holding the left button, THE Desktop Application SHALL pan the map in the direction of the drag
2. WHEN the user scrolls the mouse wheel forward, THE Desktop Application SHALL zoom in on the map with the zoom center at the mouse cursor position
3. WHEN the user scrolls the mouse wheel backward, THE Desktop Application SHALL zoom out from the map with the zoom center at the mouse cursor position
4. THE Desktop Application SHALL support a minimum zoom range from 25% to 400% of the original map size
5. THE Desktop Application SHALL render zoom and pan operations with a frame rate of at least 30 frames per second on hardware meeting minimum ATS system requirements

### Requirement 7: Real-Time Player Marker

**User Story:** As a user, I want to see a marker on the map that shows my truck's current position and the direction I'm facing, so that I can track my movement in real-time.

#### Acceptance Criteria

1. THE Desktop Application SHALL display a Player Marker as a distinct visual icon on the map
2. THE Desktop Application SHALL rotate the Player Marker icon to match the player's heading angle from the telemetry data
3. WHEN the player's position changes in ATS, THE Desktop Application SHALL update the Player Marker position within 200 milliseconds
4. THE Desktop Application SHALL render the Player Marker with sufficient contrast against the map background to ensure visibility in all map regions
5. THE Desktop Application SHALL keep the Player Marker visible and distinguishable at all supported zoom levels

### Requirement 8: Smooth Position Updates

**User Story:** As a user, I want the player marker to move smoothly across the map without jittering or jumping, so that the visual experience is pleasant and easy to follow.

#### Acceptance Criteria

1. THE Desktop Application SHALL apply interpolation or smoothing to Player Marker position updates to reduce visual jitter
2. WHILE the player is driving at constant speed in ATS, THE Desktop Application SHALL display smooth, continuous Player Marker movement without visible stuttering
3. THE Desktop Application SHALL update the Player Marker visual position at a minimum rate of 30 frames per second
4. THE Desktop Application SHALL NOT introduce position lag exceeding 500 milliseconds between the actual game position and the displayed marker position
5. THE Desktop Application documentation SHALL explain the smoothing algorithm used and provide configuration options if applicable

### Requirement 9: Application Architecture

**User Story:** As a developer, I want a clear, modular architecture with separated concerns, so that I can understand, maintain, and extend the codebase easily.

#### Acceptance Criteria

1. THE Desktop Application SHALL implement a Telemetry Layer responsible solely for reading data from the Telemetry Plugin
2. THE Desktop Application SHALL implement a Bridge Service or data layer that transforms raw telemetry data into application-specific data models
3. THE Desktop Application SHALL implement a UI Layer responsible solely for rendering the map and handling user interactions
4. THE Desktop Application SHALL define clear interfaces or contracts between the Telemetry Layer, Bridge Service, and UI Layer
5. THE Desktop Application documentation SHALL include an architecture diagram showing the relationships between all major components

### Requirement 10: Development Environment Setup

**User Story:** As a beginner, I want detailed instructions for setting up my development environment, so that I can start building the application without getting stuck on tooling issues.

#### Acceptance Criteria

1. THE Desktop Application documentation SHALL list all required software tools with specific version numbers
2. THE Desktop Application documentation SHALL provide step-by-step installation instructions for each required tool
3. THE Desktop Application documentation SHALL include instructions for obtaining the ATS Telemetry SDK
4. THE Desktop Application documentation SHALL specify the minimum and recommended hardware requirements for development
5. THE Desktop Application documentation SHALL include verification steps to confirm the development environment is correctly configured

### Requirement 11: Starter Code and Examples

**User Story:** As a developer learning a new framework, I want complete, working code examples for each major component, so that I can understand the implementation patterns and adapt them to my needs.

#### Acceptance Criteria

1. THE Desktop Application documentation SHALL provide a complete code example for reading telemetry data from the Telemetry Plugin
2. THE Desktop Application documentation SHALL provide a complete code example for the Bridge Service that processes telemetry data
3. THE Desktop Application documentation SHALL provide a complete code example for a basic desktop window that displays a map image
4. THE Desktop Application documentation SHALL provide a complete code example for projecting coordinates and drawing the Player Marker
5. THE Desktop Application documentation SHALL include inline code comments explaining key concepts and design decisions

### Requirement 12: Project Structure

**User Story:** As a developer, I want a well-organized project folder structure, so that I can easily locate files and understand the codebase organization.

#### Acceptance Criteria

1. THE Desktop Application documentation SHALL propose a complete folder structure for the project
2. THE Desktop Application project structure SHALL separate telemetry code, application logic, UI code, and map assets into distinct directories
3. THE Desktop Application documentation SHALL explain the purpose of each major folder in the project structure
4. THE Desktop Application documentation SHALL include build and run instructions that reference the project structure
5. THE Desktop Application project structure SHALL include a configuration folder for settings such as map asset paths and telemetry connection parameters

### Requirement 13: Performance Optimization

**User Story:** As a user, I want the application to run smoothly without consuming excessive system resources, so that it doesn't impact my ATS gameplay performance.

#### Acceptance Criteria

1. THE Desktop Application SHALL NOT block the UI thread during telemetry data reads or map rendering operations
2. WHERE the map uses a single large image, THE Desktop Application SHALL implement image caching to avoid repeated loading from disk
3. WHERE the map uses a tiled system, THE Desktop Application SHALL load only the tiles visible in the current viewport plus a configurable margin
4. THE Desktop Application SHALL limit CPU usage to no more than 10% on hardware meeting minimum ATS system requirements when the map is static
5. THE Desktop Application documentation SHALL explain performance optimization techniques including threading, async patterns, and resource caching

### Requirement 14: Error Handling and Diagnostics

**User Story:** As a user, I want clear error messages when something goes wrong, so that I can troubleshoot issues without needing deep technical knowledge.

#### Acceptance Criteria

1. IF the Telemetry Plugin is not loaded by ATS, THEN THE Desktop Application SHALL display an error message with instructions for installing the plugin
2. IF the Map Projection produces coordinates outside the map bounds, THEN THE Desktop Application SHALL log a warning and display the Player Marker at the map edge with a visual indicator
3. IF map asset files are missing or cannot be loaded, THEN THE Desktop Application SHALL display an error message specifying which files are missing and where they should be located
4. THE Desktop Application SHALL include a diagnostic mode that logs telemetry data values and coordinate transformations to a file
5. THE Desktop Application documentation SHALL include a troubleshooting section covering common problems and their solutions

### Requirement 15: Future Extensibility

**User Story:** As a developer, I want the application designed to support future enhancements, so that I can add features like job information, truck stats, and route visualization without major refactoring.

#### Acceptance Criteria

1. THE Desktop Application architecture SHALL support adding additional telemetry data fields without modifying the core Telemetry Layer interface
2. THE Desktop Application UI Layer SHALL support adding overlay elements (such as HUD displays) without modifying the map rendering code
3. THE Desktop Application documentation SHALL describe potential future enhancements including job information display, truck statistics HUD, route line drawing, and explored area tracking
4. THE Desktop Application documentation SHALL explain the architectural patterns that enable these future extensions
5. THE Desktop Application data models SHALL include extension points for additional properties without breaking existing functionality
