# Implementation Plan

This implementation plan breaks down the ProHauler rebranding, cleanup, optimization, and distribution work into discrete, actionable coding tasks. Each task builds incrementally, following the four-phase approach defined in the design document.

## Phase 1: Application Rebranding

- [x] 1. Rename solution and project files





  - Rename `ATSLiveMap.sln` to `ProHauler.sln`
  - Rename directory `src/ATSLiveMap.Core/` to `src/ProHauler.Core/`
  - Rename directory `src/ATSLiveMap.Telemetry/` to `src/ProHauler.Telemetry/`
  - Rename directory `src/ATSLiveMap.UI/` to `src/ProHauler.UI/`
  - Rename all `.csproj` files to match new project names
  - Update solution file to reference new project paths
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 2. Update project file configurations





  - Update `ProHauler.Core.csproj` AssemblyName and RootNamespace to "ProHauler.Core"
  - Update `ProHauler.Telemetry.csproj` AssemblyName and RootNamespace to "ProHauler.Telemetry"
  - Update `ProHauler.UI.csproj` AssemblyName and RootNamespace to "ProHauler.UI"
  - Update all ProjectReference paths in .csproj files to point to renamed projects
  - _Requirements: 1.4_

- [x] 3. Update namespaces in all C# source files





  - Replace all `namespace ATSLiveMap.Core` with `namespace ProHauler.Core` in Core project
  - Replace all `namespace ATSLiveMap.Telemetry` with `namespace ProHauler.Telemetry` in Telemetry project
  - Replace all `namespace ATSLiveMap.UI` with `namespace ProHauler.UI` in UI project
  - Update all `using ATSLiveMap.*` statements to `using ProHauler.*` across all files
  - _Requirements: 1.3_

- [x] 4. Update XAML namespace declarations





  - Update all `xmlns:local="clr-namespace:ATSLiveMap.UI"` to `xmlns:local="clr-namespace:ProHauler.UI"`
  - Update any other XAML namespace references to use ProHauler naming
  - Update x:Class attributes in XAML files to use ProHauler namespaces
  - _Requirements: 1.3_

- [x] 5. Update window titles and UI branding





  - Update main window title to "ProHauler"
  - Update PerformanceWindow title to "ProHauler - Performance Dashboard"
  - Update any About dialog or version display to show "ProHauler"
  - Update application icon if needed (can use placeholder for now)
  - _Requirements: 1.5_

- [x] 6. Add assembly metadata and version information





  - Create or update AssemblyInfo.cs with ProHauler branding
  - Set AssemblyTitle to "ProHauler"
  - Set AssemblyProduct to "ProHauler"
  - Set AssemblyDescription to "Performance tracking dashboard for American Truck Simulator"
  - Set initial version to "1.0.0.0"
  - _Requirements: 8.1, 8.2, 8.4_

- [x] 7. Verify rebranding compilation




  - Clean solution
  - Rebuild all projects in Release configuration
  - Fix any compilation errors related to namespace changes
  - Run application and verify window titles display correctly
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

## Phase 2: Code and Asset Cleanup

- [x] 8. Remove map-related assets





  - Delete `assets/maps/` directory and all contents
  - Delete any map configuration files from `assets/config/`
  - Remove `assets/` directory entirely if empty after cleanup
  - _Requirements: 2.1, 2.2_

- [x] 9. Remove obsolete scripts




  - Delete `scripts/download-full-ats-map.ps1`
  - Delete `scripts/download-hires-map.ps1`
  - Delete `scripts/download-map.ps1`
  - Delete `scripts/download-ultra-hires-map.ps1`
  - Delete `scripts/` directory if empty
  - _Requirements: 2.4_

- [x] 10. Identify and remove map-related code





  - Search codebase for classes/files containing "Map" in name
  - Remove MapWindow.xaml and MapWindow.xaml.cs if they exist
  - Remove any map rendering controls or components
  - Remove map coordinate transformation code
  - Remove map image loading logic
  - _Requirements: 2.2_

- [ ] 11. Remove unused test projects



  - Check if `src/ATSLiveMap.TestConsole/` exists and is unused
  - Remove TestConsole project from solution if not needed
  - Delete TestConsole directory if removed from solution
  - _Requirements: 2.2_

- [x] 12. Clean up configuration files





  - Update `appsettings.json` to remove map-related settings
  - Remove map file path configurations
  - Keep telemetry, database, logging, and performance settings
  - Ensure database path uses ProHauler directory: `%LOCALAPPDATA%\\ProHauler\\trips.db`
  - _Requirements: 2.2, 3.3_
-

- [x] 13. Review and remove unused NuGet packages




  - Check for map-related or image processing libraries
  - Remove any packages only used for map functionality
  - Keep essential packages: MathNet.Numerics, Serilog, System.Text.Json, SQLite, LiveCharts, DI packages
  - _Requirements: 2.5_


- [ ] 14. Verify application functionality after cleanup



  - Build solution
  - Launch ProHauler application
  - Verify performance dashboard loads without errors
  - Test telemetry connection (if ATS is running)
  - Verify all performance metrics display correctly
  - Test trip history and charts
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

## Phase 3: Code Optimization

- [x] 15. Remove unused using statements





  - Use IDE "Remove Unused Usings" feature on all .cs files in ProHauler.Core
  - Use IDE "Remove Unused Usings" feature on all .cs files in ProHauler.Telemetry
  - Use IDE "Remove Unused Usings" feature on all .cs files in ProHauler.UI
  - _Requirements: 4.1_

- [x] 16. Add XML documentation to public APIs




  - Add `<summary>` tags to all public classes in ProHauler.Core
  - Add `<summary>`, `<param>`, and `<returns>` tags to all public methods
  - Document PerformanceCalculator algorithm details
  - Document TripRepository methods
  - Document all model classes (PerformanceMetrics, Trip, etc.)
  - _Requirements: 4.5_

- [x] 17. Apply consistent code formatting





  - Run code formatter on all .cs files
  - Ensure consistent indentation (4 spaces)
  - Ensure consistent brace style
  - Remove any commented-out code blocks
  - _Requirements: 4.4_

- [x] 18. Consolidate duplicate code





  - Review PerformanceCalculator for duplicate logic
  - Extract common validation logic into helper methods
  - Review UI code for duplicate styling or logic
  - Consolidate similar patterns into reusable methods
  - _Requirements: 4.3_

- [x] 19. Run code analysis and fix warnings





  - Enable code analysis in project files
  - Run analysis on all projects
  - Fix any critical or high-priority warnings
  - Document any warnings that are intentionally ignored
  - _Requirements: 4.2_

## Phase 4: Windows Installer Creation

- [x] 20. Set up WiX Toolset project





  - Install WiX Toolset v4 (or v3 if v4 not stable)
  - Create `installer/` directory in solution root
  - Create new WiX installer project `ProHauler.Installer.wixproj`
  - Add installer project to solution
  - _Requirements: 5.1, 9.1_

- [x] 21. Configure installer product information





  - Create `Product.wxs` with product definition
  - Set Product Name to "ProHauler"
  - Set Manufacturer to Dorian Smith
  - Generate and set UpgradeCode GUID
  - Set initial version to "1.0.0"
  - Configure for per-machine installation
  - _Requirements: 5.1, 8.2, 8.3_

- [x] 22. Define file installation components





  - Create `Files.wxs` for file components
  - Define component for ProHauler.exe
  - Define components for all DLL dependencies
  - Define component for appsettings.json
  - Set installation directory to `[ProgramFilesFolder]ProHauler`
  - _Requirements: 5.2, 6.2_

- [ ] 23. Configure self-contained publishing
  - Update ProHauler.UI.csproj with publish settings
  - Set PublishSingleFile to true
  - Set SelfContained to true
  - Set RuntimeIdentifier to win-x64
  - Set PublishReadyToRun to true
  - Set IncludeNativeLibrariesForSelfExtract to true
  - _Requirements: 6.1_

- [ ] 24. Create Start Menu and Desktop shortcuts
  - Create `Shortcuts.wxs` for shortcut definitions
  - Define Start Menu shortcut pointing to ProHauler.exe
  - Define optional Desktop shortcut
  - Set shortcut names to "ProHauler"
  - Set working directory to installation folder
  - _Requirements: 5.3, 5.4_

- [ ] 25. Configure registry entries for Add/Remove Programs
  - Create `Registry.wxs` for registry entries
  - Add DisplayName: "ProHauler"
  - Add DisplayVersion from product version
  - Add Publisher information
  - Add InstallLocation
  - Add UninstallString
  - _Requirements: 5.5_

- [ ] 26. Create application data directory structure
  - Add custom action to create `%LOCALAPPDATA%\ProHauler` directory
  - Create `logs\` subdirectory
  - Set appropriate permissions for user data directory
  - _Requirements: 6.4_

- [ ] 27. Design installer UI dialogs
  - Create `UI.wxs` for custom UI
  - Add welcome dialog with ProHauler branding
  - Add installation directory selection dialog
  - Add shortcuts selection dialog (Desktop shortcut checkbox)
  - Add telemetry plugin setup instructions dialog
  - Add installation progress dialog
  - Add completion dialog with "Launch ProHauler" option
  - _Requirements: 7.1, 7.2_

- [ ] 28. Create automated build script
  - Create `build-installer.ps1` PowerShell script
  - Add step to clean previous builds
  - Add step to restore NuGet packages
  - Add step to build solution in Release configuration
  - Add step to publish self-contained application
  - Add step to build WiX installer project
  - Add step to copy output MSI to predictable location
  - Add error handling and clear error messages
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [ ] 29. Test installer on clean Windows environment
  - Run installer on clean Windows 10 VM
  - Verify installation completes without errors
  - Verify files installed to Program Files\ProHauler
  - Verify Start Menu shortcut created and works
  - Verify Desktop shortcut created if selected
  - Launch ProHauler from shortcut
  - Verify application creates database in AppData\Local\ProHauler
  - Verify Add/Remove Programs entry exists
  - Test uninstaller and verify complete removal
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [ ] 30. Test installer on Windows 11
  - Repeat all installation tests on Windows 11 VM
  - Verify compatibility and functionality
  - _Requirements: 10.1_

## Phase 5: Documentation and Release

- [ ] 31. Update README.md
  - Replace title with "ProHauler"
  - Update description to focus on performance tracking
  - Remove all map-related feature descriptions
  - Add performance tracking features list
  - Update installation instructions to reference installer
  - Add system requirements section
  - Update troubleshooting section for performance tracking
  - Remove map-related troubleshooting
  - Add link to installer download
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [ ] 32. Update TELEMETRY-SETUP.md
  - Update all references from ATSLiveMap to ProHauler
  - Ensure instructions are clear and accurate
  - Add screenshots if possible
  - _Requirements: 3.5, 7.5_

- [ ] 33. Create release notes document
  - Document version 1.0.0 features
  - List all performance metrics
  - Describe trip history and analytics features
  - Include installation instructions
  - Include telemetry plugin setup steps
  - _Requirements: 3.1, 3.2_

- [ ] 34. Prepare release artifacts
  - Build final installer using build script
  - Rename installer to `ProHauler-Setup-v1.0.0.msi`
  - Copy README.md to release folder
  - Copy TELEMETRY-SETUP.md to release folder
  - Copy release notes to release folder
  - Create LICENSE file if needed
  - _Requirements: 5.1, 9.3_

- [ ] 35. Final verification checklist
  - Verify all "ATSLiveMap" references removed from codebase
  - Verify all map-related code removed
  - Verify code is optimized and documented
  - Verify version numbers are correct in all assemblies
  - Verify README.md is accurate and complete
  - Verify installer works on both Windows 10 and 11
  - Verify application functionality post-install
  - Verify uninstaller works correctly
  - _Requirements: 1.1-1.5, 2.1-2.5, 3.1-3.5, 4.1-4.5, 5.1-5.5, 10.1-10.5_

## Notes

- Tasks marked with `*` are optional and can be skipped to focus on core functionality
- Each task references specific requirements from the requirements document
- Complete Phase 1 before moving to Phase 2
- Complete Phase 2 before moving to Phase 3
- Complete Phase 3 before moving to Phase 4
- Phase 5 can be done in parallel with Phase 4 testing
- Estimated total time: 10-15 hours
- Test thoroughly on clean VMs before distributing to users
