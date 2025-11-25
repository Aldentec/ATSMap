# Requirements Document

## Introduction

This specification defines the requirements for preparing the ProHauler application for public distribution. The system must be rebranded from its original "ATSLiveMap" naming to "ProHauler", cleaned of obsolete code and map-related artifacts, optimized for performance and maintainability, and packaged with a professional Windows installer for easy end-user installation.

## Glossary

- **ProHauler**: The performance tracking dashboard application for American Truck Simulator
- **Application**: The ProHauler desktop software
- **Installer**: A Windows installation package that deploys ProHauler to end-user systems
- **Codebase**: All source code, configuration files, and project assets
- **Artifact**: Build output, compiled binaries, or deployment packages
- **Telemetry Plugin**: The scs-sdk-plugin DLL that provides game data to ProHauler
- **Distribution Package**: The complete installer bundle ready for sharing with end users

## Requirements

### Requirement 1: Application Rebranding

**User Story:** As a developer, I want to rename all "ATSLiveMap" references to "ProHauler" throughout the codebase, so that the application name accurately reflects its current functionality.

#### Acceptance Criteria

1. THE Application SHALL rename all solution files from "ATSLiveMap.sln" to "ProHauler.sln"
2. THE Application SHALL rename all project folders from "ATSLiveMap.*" pattern to "ProHauler.*" pattern
3. THE Application SHALL rename all namespaces from "ATSLiveMap" to "ProHauler" in all source files
4. THE Application SHALL update all assembly names and root namespaces in project files to use "ProHauler" naming
5. THE Application SHALL update the window title to display "ProHauler" instead of map-related titles

### Requirement 2: Obsolete Code Removal

**User Story:** As a developer, I want to remove all map-related code and unused files from the codebase, so that the application contains only relevant functionality and is easier to maintain.

#### Acceptance Criteria

1. THE Application SHALL remove all map image files from the assets/maps directory
2. THE Application SHALL delete all map rendering code from the UI layer
3. THE Application SHALL remove all map configuration files from assets/config directory
4. THE Application SHALL delete unused PowerShell scripts for map downloading from the scripts directory
5. THE Application SHALL remove any map-related dependencies from project files that are no longer needed

### Requirement 3: Documentation Updates

**User Story:** As an end user, I want clear and accurate documentation that describes ProHauler's functionality, so that I can understand how to install and use the application.

#### Acceptance Criteria

1. THE Application SHALL update README.md to describe ProHauler as a performance tracking dashboard for American Truck Simulator
2. THE Application SHALL provide installation instructions that reference the ProHauler installer package
3. THE Application SHALL document all performance metrics and their calculation methods
4. THE Application SHALL include troubleshooting steps specific to performance tracking functionality
5. THE Application SHALL remove all map-related documentation and screenshots

### Requirement 4: Code Optimization

**User Story:** As a developer, I want to optimize the codebase for performance and maintainability, so that the application runs efficiently and is easy to enhance in the future.

#### Acceptance Criteria

1. THE Application SHALL remove unused using statements from all source files
2. THE Application SHALL eliminate dead code paths that are no longer executed
3. THE Application SHALL consolidate duplicate code into reusable methods or services
4. THE Application SHALL apply consistent code formatting across all source files
5. THE Application SHALL ensure all public APIs have XML documentation comments

### Requirement 5: Windows Installer Creation

**User Story:** As an end user, I want a professional Windows installer, so that I can easily install ProHauler on my computer without manual file copying or configuration.

#### Acceptance Criteria

1. THE Application SHALL provide a Windows installer package in MSI or EXE format
2. WHEN the user runs the installer, THE Installer SHALL install ProHauler to Program Files directory
3. THE Installer SHALL create a Start Menu shortcut named "ProHauler" for launching the Application
4. THE Installer SHALL create a Desktop shortcut if the user opts in during installation
5. THE Installer SHALL register ProHauler for proper uninstallation through Windows Settings

### Requirement 6: Dependency Bundling

**User Story:** As an end user, I want all required dependencies included in the installer, so that I don't need to manually install .NET runtime or other prerequisites.

#### Acceptance Criteria

1. THE Installer SHALL include the .NET 8.0 Desktop Runtime or bundle the application as self-contained
2. THE Installer SHALL include all required NuGet package dependencies
3. THE Installer SHALL copy necessary configuration files to the installation directory
4. THE Installer SHALL create the application data directory structure if it doesn't exist
5. THE Installer SHALL verify that all dependencies are present before completing installation

### Requirement 7: Telemetry Plugin Setup

**User Story:** As an end user, I want clear guidance on installing the telemetry plugin, so that ProHauler can connect to American Truck Simulator.

#### Acceptance Criteria

1. THE Installer SHALL display instructions for downloading and installing the scs-telemetry.dll plugin
2. THE Installer SHALL provide a direct link to the scs-sdk-plugin releases page
3. THE Application SHALL detect if the telemetry plugin is not installed and display helpful guidance
4. THE Application SHALL provide a "Help" menu item with telemetry setup instructions
5. THE Documentation SHALL include step-by-step screenshots for plugin installation

### Requirement 8: Version Management

**User Story:** As a developer, I want proper version numbering throughout the application, so that users and I can track which version is installed and identify updates.

#### Acceptance Criteria

1. THE Application SHALL display version number in the About dialog or window title
2. THE Application SHALL use semantic versioning format (MAJOR.MINOR.PATCH)
3. THE Installer SHALL display the version number during installation
4. THE Application SHALL store version information in assembly metadata
5. THE Application SHALL include version number in exported CSV files for traceability

### Requirement 9: Build Automation

**User Story:** As a developer, I want an automated build script that creates the installer, so that I can reliably produce distribution packages without manual steps.

#### Acceptance Criteria

1. THE Build System SHALL provide a script or batch file that builds the release configuration
2. THE Build System SHALL run the installer creation tool automatically after successful build
3. THE Build System SHALL output the installer package to a predictable location
4. THE Build System SHALL fail with clear error messages if any step fails
5. THE Build System SHALL clean previous build artifacts before creating new ones

### Requirement 10: Installation Testing

**User Story:** As a developer, I want to verify that the installer works correctly, so that end users have a smooth installation experience.

#### Acceptance Criteria

1. THE Installer SHALL complete installation without errors on a clean Windows 10/11 system
2. WHEN installation completes, THE Application SHALL launch successfully from the Start Menu shortcut
3. THE Application SHALL connect to ATS telemetry when the game is running with the plugin installed
4. THE Application SHALL create and access its database file in the correct location
5. THE Uninstaller SHALL remove all application files and shortcuts when executed
