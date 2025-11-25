# Design Document

## Overview

This design document outlines the technical approach for rebranding the application from "ATSLiveMap" to "ProHauler", removing obsolete map-related code, optimizing the codebase, and creating a professional Windows installer for distribution. The work is divided into four major phases: Rebranding, Cleanup, Optimization, and Distribution.

## Architecture

### Current Structure
```
ATSLiveMap/
├── src/
│   ├── ATSLiveMap.Core/          # Business logic, services, models
│   ├── ATSLiveMap.Telemetry/     # Telemetry client
│   ├── ATSLiveMap.UI/            # WPF application
│   └── ATSLiveMap.TestConsole/   # Test console (may be unused)
├── assets/
│   ├── maps/                     # Map images (to be removed)
│   └── config/                   # Configuration files
├── scripts/                      # Map download scripts (to be removed)
└── ATSLiveMap.sln
```

### Target Structure
```
ProHauler/
├── src/
│   ├── ProHauler.Core/           # Business logic, services, models
│   ├── ProHauler.Telemetry/      # Telemetry client
│   └── ProHauler.UI/             # WPF application
├── assets/
│   └── config/                   # Configuration files only
├── installer/                    # Installer project files
└── ProHauler.sln
```

## Components and Interfaces

### Phase 1: Rebranding Strategy

#### 1.1 Namespace Renaming
**Approach:** Use automated find-and-replace with verification

- **Namespaces to rename:**
  - `ATSLiveMap.Core` → `ProHauler.Core`
  - `ATSLiveMap.Telemetry` → `ProHauler.Telemetry`
  - `ATSLiveMap.UI` → `ProHauler.UI`

- **Files affected:**
  - All `.cs` files (using statements, namespace declarations)
  - All `.csproj` files (RootNamespace, AssemblyName)
  - All `.xaml` files (xmlns declarations)
  - Solution file `.sln`

**Implementation:**
1. Use PowerShell script to perform bulk rename in file contents
2. Manually rename project directories
3. Update solution file references
4. Rebuild and verify no compilation errors

#### 1.2 Assembly and Project Renaming
**Files to rename:**
- `ATSLiveMap.sln` → `ProHauler.sln`
- `src/ATSLiveMap.Core/` → `src/ProHauler.Core/`
- `src/ATSLiveMap.Telemetry/` → `src/ProHauler.Telemetry/`
- `src/ATSLiveMap.UI/` → `src/ProHauler.UI/`
- `*.csproj` files within each directory

**Project file updates:**
```xml
<PropertyGroup>
  <AssemblyName>ProHauler.Core</AssemblyName>
  <RootNamespace>ProHauler.Core</RootNamespace>
</PropertyGroup>
```

#### 1.3 UI Branding Updates
**Window titles:**
- Main window: "ProHauler"
- Performance window: "ProHauler - Performance Dashboard"

**Application metadata:**
- Assembly title: "ProHauler"
- Assembly product: "ProHauler"
- Assembly description: "Performance tracking dashboard for American Truck Simulator"

### Phase 2: Code Cleanup

#### 2.1 Files to Delete

**Map-related assets:**
- `assets/maps/` (entire directory)
- `assets/config/map-*.json` (any map configuration files)

**Obsolete scripts:**
- `scripts/download-full-ats-map.ps1`
- `scripts/download-hires-map.ps1`
- `scripts/download-map.ps1`
- `scripts/download-ultra-hires-map.ps1`
- Entire `scripts/` directory if empty after cleanup

**Test projects (if unused):**
- `src/ATSLiveMap.TestConsole/` (verify it's not needed first)

#### 2.2 Code to Remove

**Map rendering code:**
- Search for classes/files containing "Map" in name (excluding "MapView" if it's repurposed)
- Remove map image loading logic
- Remove map coordinate transformation code
- Remove map zoom/pan controls if they're map-specific

**Unused dependencies:**
- Review NuGet packages for map-related libraries
- Remove any image processing libraries only used for maps
- Keep: MathNet.Numerics (used for calculations), Serilog, System.Text.Json, SQLite, LiveCharts

#### 2.3 Configuration Cleanup

**appsettings.json:**
- Remove map file path configurations
- Remove map-related settings
- Keep telemetry endpoint, database path, logging settings

### Phase 3: Code Optimization

#### 3.1 Code Quality Improvements

**Unused using statements:**
- Use Visual Studio's "Remove Unused Usings" feature
- Apply to all `.cs` files in solution

**Dead code elimination:**
- Use code coverage tools to identify unused methods
- Remove commented-out code blocks
- Remove unused private methods and fields

**Code consolidation:**
- Identify duplicate code patterns in PerformanceCalculator
- Extract common validation logic into helper methods
- Consolidate similar UI styling code into shared resources

#### 3.2 Documentation Standards

**XML documentation:**
- Ensure all public classes have `<summary>` tags
- Ensure all public methods have `<summary>`, `<param>`, and `<returns>` tags
- Document complex algorithms (smoothness calculation, fuel efficiency)

**Code formatting:**
- Apply consistent indentation (4 spaces)
- Ensure consistent brace style
- Use EditorConfig for consistency

### Phase 4: Installer Creation

#### 4.1 Installer Technology Selection

**Recommended: WiX Toolset v4**
- Industry standard for Windows installers
- Creates MSI packages
- Supports custom UI
- Integrates with Visual Studio

**Alternative: Inno Setup**
- Simpler scripting
- Creates EXE installers
- Good for smaller applications
- Free and open source

**Decision:** Use WiX Toolset for professional MSI installer

#### 4.2 Installer Components

**Installation structure:**
```
C:\Program Files\ProHauler\
├── ProHauler.exe
├── ProHauler.Core.dll
├── ProHauler.Telemetry.dll
├── appsettings.json
├── [All NuGet dependencies]
└── [.NET runtime if self-contained]
```

**User data structure:**
```
C:\Users\[Username]\AppData\Local\ProHauler\
├── trips.db
└── logs\
```

#### 4.3 Installer Features

**Installation wizard:**
1. Welcome screen with ProHauler branding
2. License agreement (if applicable)
3. Installation directory selection (default: Program Files\ProHauler)
4. Shortcuts selection:
   - Start Menu shortcut (required)
   - Desktop shortcut (optional, checked by default)
5. Telemetry plugin setup instructions screen
6. Installation progress
7. Completion screen with "Launch ProHauler" option

**Registry entries:**
- Add/Remove Programs registration
- Version information
- Installation path
- Uninstaller path

**Shortcuts:**
- Start Menu: `ProHauler`
- Desktop: `ProHauler` (optional)
- Both point to: `C:\Program Files\ProHauler\ProHauler.exe`

#### 4.4 Deployment Strategy

**Self-contained vs Framework-dependent:**

**Option 1: Self-contained (Recommended)**
- Includes .NET 8.0 Desktop Runtime
- Larger installer (~150MB)
- No prerequisites for users
- Easier installation experience

**Option 2: Framework-dependent**
- Requires .NET 8.0 Desktop Runtime pre-installed
- Smaller installer (~10MB)
- Installer must check for runtime and prompt download if missing

**Decision:** Self-contained for better user experience

**Publishing configuration:**
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishReadyToRun>true</PublishReadyToRun>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

#### 4.5 WiX Installer Project Structure

**New project: `installer/ProHauler.Installer.wixproj`**

**Key WiX files:**
- `Product.wxs` - Main installer definition
- `UI.wxs` - Custom UI dialogs
- `Files.wxs` - File installation components
- `Registry.wxs` - Registry entries
- `Shortcuts.wxs` - Start Menu and Desktop shortcuts

**Product.wxs structure:**
```xml
<Product Id="*" 
         Name="ProHauler" 
         Version="1.0.0"
         Manufacturer="[Your Name/Company]"
         UpgradeCode="[GUID]">
  
  <Package InstallerVersion="500" 
           Compressed="yes" 
           InstallScope="perMachine" />
  
  <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
  
  <MediaTemplate EmbedCab="yes" />
  
  <Feature Id="ProductFeature" Title="ProHauler" Level="1">
    <ComponentGroupRef Id="ProductComponents" />
    <ComponentGroupRef Id="Shortcuts" />
  </Feature>
</Product>
```

#### 4.6 Build Automation

**Build script: `build-installer.ps1`**

```powershell
# Clean previous builds
Remove-Item -Recurse -Force .\publish -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\installer\bin -ErrorAction SilentlyContinue

# Restore and build solution
dotnet restore ProHauler.sln
dotnet build ProHauler.sln --configuration Release

# Publish self-contained application
dotnet publish src/ProHauler.UI/ProHauler.UI.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output .\publish `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true

# Build WiX installer
msbuild installer/ProHauler.Installer.wixproj `
  /p:Configuration=Release `
  /p:OutputPath=..\output

Write-Host "Installer created: output\ProHauler.msi"
```

## Data Models

### Version Information

**AssemblyInfo attributes:**
```csharp
[assembly: AssemblyTitle("ProHauler")]
[assembly: AssemblyDescription("Performance tracking dashboard for American Truck Simulator")]
[assembly: AssemblyProduct("ProHauler")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```

**Version display in UI:**
- Add version label to About dialog or status bar
- Format: "ProHauler v1.0.0"
- Read from assembly metadata at runtime

### Configuration Schema

**appsettings.json (cleaned):**
```json
{
  "Telemetry": {
    "EndpointUrl": "http://localhost:25555",
    "PollingIntervalMs": 100,
    "ReconnectDelayMs": 5000
  },
  "Database": {
    "FilePath": "%LOCALAPPDATA%\\ProHauler\\trips.db"
  },
  "Logging": {
    "LogLevel": "Information",
    "LogPath": "%LOCALAPPDATA%\\ProHauler\\logs"
  },
  "Performance": {
    "FuelEfficiencyBaseline": 6.0,
    "DefaultSpeedLimit": 55,
    "HighwaySpeedLimit": 65
  }
}
```

## Error Handling

### Installer Error Handling

**Common installation errors:**
1. Insufficient permissions → Prompt for administrator elevation
2. Disk space insufficient → Check before installation, show error
3. .NET runtime missing (framework-dependent) → Provide download link
4. Installation directory locked → Suggest closing applications

**Rollback strategy:**
- WiX automatically handles rollback on installation failure
- Ensure no files are left behind in Program Files
- Clean up any created registry entries

### Application Error Handling

**First-run detection:**
- Check if database exists
- Create database and directory structure if missing
- Show welcome dialog with telemetry setup instructions

**Telemetry plugin detection:**
- Check for plugin at expected path on startup
- If not found, show helpful dialog with:
  - Link to download page
  - Installation instructions
  - Option to dismiss and continue (app will retry connection)

## Testing Strategy

### Rebranding Verification

**Compilation tests:**
1. Clean solution
2. Rebuild all projects
3. Verify no compilation errors
4. Run application and verify window titles

**Namespace verification:**
1. Search entire solution for "ATSLiveMap" string
2. Verify only expected occurrences remain (comments, documentation)
3. Check all using statements resolve correctly

### Cleanup Verification

**File deletion verification:**
1. Verify map files are deleted
2. Verify scripts directory is removed
3. Check git status to ensure no unintended deletions

**Functionality testing:**
1. Launch application
2. Verify performance dashboard loads
3. Connect to ATS telemetry
4. Verify all metrics calculate correctly
5. Verify trip history and charts work

### Installer Testing

**Installation test matrix:**

| Test Case | Environment | Expected Result |
|-----------|-------------|-----------------|
| Fresh install | Clean Windows 10 | Successful installation |
| Fresh install | Clean Windows 11 | Successful installation |
| Upgrade install | Previous version installed | Successful upgrade |
| Uninstall | After installation | Complete removal |
| Shortcut launch | After installation | Application launches |
| Non-admin install | Standard user account | Elevation prompt |

**Post-installation verification:**
1. Verify files in Program Files\ProHauler
2. Verify Start Menu shortcut exists and works
3. Verify Desktop shortcut exists (if selected)
4. Launch application from shortcut
5. Verify application creates database in AppData
6. Verify Add/Remove Programs entry exists
7. Uninstall and verify complete removal

### Optimization Verification

**Code quality checks:**
1. Run code analysis tools
2. Verify no unused using statements
3. Check for TODO/HACK comments
4. Verify XML documentation coverage

**Performance testing:**
1. Measure application startup time
2. Verify memory usage is reasonable
3. Test telemetry update performance (should handle 10 updates/sec)
4. Verify UI remains responsive during calculations

## Implementation Phases

### Phase 1: Rebranding (Estimated: 2-3 hours)
1. Create backup branch
2. Rename project directories
3. Update solution file
4. Update all namespaces in code files
5. Update project files (csproj)
6. Update XAML files
7. Update window titles and branding
8. Rebuild and test

### Phase 2: Cleanup (Estimated: 1-2 hours)
1. Delete map assets directory
2. Delete scripts directory
3. Remove map-related code
4. Remove unused test projects
5. Clean configuration files
6. Update .gitignore if needed
7. Test application functionality

### Phase 3: Optimization (Estimated: 2-3 hours)
1. Remove unused using statements
2. Add XML documentation
3. Apply code formatting
4. Consolidate duplicate code
5. Run code analysis
6. Fix any warnings
7. Final testing

### Phase 4: Distribution (Estimated: 4-6 hours)
1. Install WiX Toolset
2. Create installer project
3. Configure product information
4. Define file components
5. Create shortcuts
6. Design installer UI
7. Create build script
8. Test installer on clean VM
9. Create README with installation instructions
10. Package final release

## Documentation Updates

### README.md Structure

```markdown
# ProHauler

A performance tracking dashboard for American Truck Simulator that helps you improve your driving skills through real-time metrics and historical analysis.

## Features
- Real-time performance scoring (smoothness, fuel efficiency, speed compliance, safety)
- Trip history with detailed statistics
- Performance trends and analytics
- CSV export for external analysis

## Installation

### Quick Install
1. Download `ProHauler-Setup.msi` from releases
2. Run the installer and follow the wizard
3. Launch ProHauler from Start Menu

### Telemetry Plugin Setup
ProHauler requires the scs-sdk-plugin to read data from ATS:
1. Download from [releases page](link)
2. Copy to `Documents\American Truck Simulator\bin\win_x64\plugins\`
3. Launch ATS to load the plugin

## Usage
[Screenshots and usage instructions]

## System Requirements
- Windows 10/11 (64-bit)
- .NET 8.0 Desktop Runtime (included in installer)
- American Truck Simulator 1.49+

## Troubleshooting
[Common issues and solutions]
```

### TELEMETRY-SETUP.md Updates
- Update all references to application name
- Ensure instructions are clear and current
- Add screenshots if possible

## Deployment Checklist

**Pre-release:**
- [ ] All code rebranded to ProHauler
- [ ] All map-related code removed
- [ ] Code optimized and documented
- [ ] Version number set in all assemblies
- [ ] README.md updated
- [ ] TELEMETRY-SETUP.md updated
- [ ] Installer tested on clean Windows 10
- [ ] Installer tested on clean Windows 11
- [ ] Application functionality verified post-install
- [ ] Uninstaller tested

**Release artifacts:**
- [ ] ProHauler-Setup.msi (installer)
- [ ] README.md
- [ ] TELEMETRY-SETUP.md
- [ ] LICENSE (if applicable)
- [ ] Release notes

**Distribution:**
- [ ] Create GitHub release
- [ ] Upload installer to release
- [ ] Write release notes
- [ ] Share download link
