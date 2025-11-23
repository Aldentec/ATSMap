# Requirements Document

## Introduction

This document defines the requirements for creating a mod for American Truck Simulator (ATS) that increases the activation radius of rest areas globally. The mod will allow players to more easily trigger rest/sleep zones without manually editing each rest area instance. The solution will leverage prefab editing or the closest practical approach to achieve global changes across all rest areas in the game.

## Glossary

- **ATS**: American Truck Simulator, the base game being modded
- **Rest Area**: In-game locations where players can rest or sleep to restore fatigue
- **Activation Radius**: The distance from a rest area trigger point within which the rest/sleep action becomes available to the player
- **Prefab**: A reusable game object template that can be instantiated multiple times across the game world
- **Mod**: A modification package that alters game behavior or content
- **SCS**: SCS Software, the developer of American Truck Simulator
- **Trigger Zone**: The invisible area that detects player presence and activates rest functionality
- **Mod System**: The game's built-in system for loading and applying user-created modifications
- **Game Files**: The original data files containing prefabs, definitions, and map data

## Requirements

### Requirement 1

**User Story:** As a modder with no prior ATS modding experience, I want clear documentation on how rest areas work in ATS, so that I understand what needs to be modified to change the activation radius.

#### Acceptance Criteria

1. THE Mod System SHALL provide documentation explaining how rest areas are defined in ATS through prefabs, triggers, or other data structures
2. THE Mod System SHALL identify which specific files or data structures control the activation radius of rest areas
3. THE Mod System SHALL explain whether modifying prefabs can change the radius for all instances globally
4. THE Mod System SHALL document any limitations or constraints when modifying rest area prefabs
5. THE Mod System SHALL clarify the relationship between prefab definitions and map instances

### Requirement 2

**User Story:** As a beginner modder, I want step-by-step instructions for setting up all required tools, so that I can start creating the mod without prior knowledge of ATS modding tools.

#### Acceptance Criteria

1. THE Mod System SHALL provide a complete list of required tools including SCS Extractor, SCS Map Editor, and any text editors
2. THE Mod System SHALL include installation instructions for each required tool on Windows systems
3. THE Mod System SHALL explain how to configure each tool for ATS modding
4. THE Mod System SHALL verify that all tools are compatible with the latest ATS version
5. THE Mod System SHALL provide troubleshooting steps for common tool installation issues

### Requirement 3

**User Story:** As a modder, I want to extract and locate the specific prefab or definition files that control rest area radius, so that I can identify what needs to be modified.

#### Acceptance Criteria

1. WHEN extracting game files, THE Mod System SHALL identify the correct archive files containing rest area definitions
2. THE Mod System SHALL provide the exact file paths and names of rest area prefab files
3. THE Mod System SHALL explain how to search for and identify rest area trigger definitions within extracted files
4. THE Mod System SHALL document the file format and structure of rest area definitions
5. THE Mod System SHALL identify all prefab variants that need modification for complete coverage

### Requirement 4

**User Story:** As a modder, I want to modify the activation radius parameter in rest area prefabs, so that all instances in the game use the increased radius.

#### Acceptance Criteria

1. THE Mod System SHALL identify the specific parameter name that controls activation radius in prefab files
2. THE Mod System SHALL provide the default radius value and recommend an increased value for noticeable improvement
3. WHEN modifying radius values, THE Mod System SHALL preserve all other prefab properties and relationships
4. THE Mod System SHALL support editing through either the SCS Map Editor or direct text file editing
5. THE Mod System SHALL validate that modified values are within acceptable game engine limits

### Requirement 5

**User Story:** As a modder, I want to package my modifications into a proper mod structure, so that the game can load and apply my changes correctly.

#### Acceptance Criteria

1. THE Mod System SHALL define the correct folder structure for an ATS mod package
2. THE Mod System SHALL specify which modified files need to be included in the mod package
3. THE Mod System SHALL explain how to create a .scs archive file from the mod folder structure
4. THE Mod System SHALL ensure the mod references the correct file paths to override vanilla game files
5. THE Mod System SHALL include a manifest or descriptor file if required by the game

### Requirement 6

**User Story:** As a modder, I want to test my mod in-game, so that I can verify the activation radius has actually increased.

#### Acceptance Criteria

1. THE Mod System SHALL explain how to enable the mod in the ATS mod manager
2. THE Mod System SHALL provide instructions for loading a save game or starting a new game with the mod active
3. WHEN testing in-game, THE Mod System SHALL describe observable behaviors that confirm the radius increase
4. THE Mod System SHALL provide specific rest area locations suitable for testing
5. THE Mod System SHALL explain how to measure or verify the actual radius change

### Requirement 7

**User Story:** As a modder, I want to troubleshoot common issues, so that I can fix problems when the mod doesn't work as expected.

#### Acceptance Criteria

1. IF the mod fails to load, THEN THE Mod System SHALL provide diagnostic steps to identify the cause
2. THE Mod System SHALL document common mistakes such as incorrect file paths, version mismatches, and mod conflicts
3. THE Mod System SHALL explain how to check game logs for mod-related errors
4. THE Mod System SHALL provide solutions for compatibility issues with DLC content
5. THE Mod System SHALL describe how to verify mod load order and priority

### Requirement 8

**User Story:** As a modder, I want guidance on maintaining and extending the mod, so that I can keep it compatible with game updates and add new features.

#### Acceptance Criteria

1. THE Mod System SHALL explain how game updates may affect the mod and require updates
2. THE Mod System SHALL provide best practices for version compatibility and mod maintenance
3. THE Mod System SHALL suggest potential extensions such as different radii for different rest area types
4. THE Mod System SHALL explain how to create optional variants of the mod with different radius values
5. THE Mod System SHALL document how to share the mod with other players if desired
