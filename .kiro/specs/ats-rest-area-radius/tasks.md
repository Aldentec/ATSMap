# Implementation Plan

- [x] 1. Set up development environment and tools





  - Download and configure SCS Extractor for extracting ATS game files
  - Install 7-Zip for creating .scs archive files
  - Set up text editor (VS Code or Notepad++) with UTF-8 encoding support
  - Create working directory structure for extracted files and mod development
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [x] 2. Extract and analyze game files







  - Locate ATS installation directory via Steam
  - Extract base.scs and def.scs archives using SCS Extractor
  - Navigate to /def/world/prefab/ directory in extracted files
  - Search for rest area related prefab files (rest_area_*.sii, parking_*.sii, service_*.sii)
  - Document all identified rest area prefab files and their locations
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [ ] 3. Identify trigger zone parameters in prefab files
  - Open identified prefab .sii files in text editor
  - Search for trigger_def blocks within prefab definitions
  - Locate radius and activation_distance parameters
  - Document current default values for each rest area type
  - Verify trigger_type is sphere or cylinder for rest areas
  - Create backup copies of original files for reference
  - _Requirements: 3.3, 3.4, 4.1, 4.2_

- [ ] 4. Modify rest area prefab definitions
  - Create mod folder structure: ATS_Rest_Area_Mod/def/world/prefab/
  - Copy identified rest area prefab files to mod folder structure
  - Edit radius parameter values (increase from ~15.0 to 30.0)
  - Edit activation_distance parameter values to match radius
  - Verify all syntax remains valid (brackets, semicolons, formatting)
  - Ensure UTF-8 encoding is preserved
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [ ] 5. Create mod manifest and metadata
  - Create manifest.sii file in mod root directory
  - Write mod package definition with version, name, author, and description
  - Set category to "map" for proper mod manager categorization
  - Add compatible_versions array with current ATS version pattern (e.g., "1.50.*")
  - Create or add mod_icon.jpg file (optional but recommended)
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 6. Package mod into .scs archive
  - Verify complete folder structure matches vanilla game paths
  - Use 7-Zip to create ZIP archive of mod folder contents
  - Rename .zip file to .scs extension
  - Verify archive contains correct folder hierarchy (def/world/prefab/ at root)
  - Move .scs file to ATS mod directory (Documents\American Truck Simulator\mod\)
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ] 7. Test mod loading and functionality
  - Launch ATS and open mod manager
  - Verify mod appears in mod list with correct name and description
  - Enable the mod and set appropriate priority
  - Start game and check game.log.txt for errors
  - Drive to test rest area location (e.g., I-5 California rest stop)
  - Approach rest area from multiple angles and verify increased activation distance
  - Compare activation distance with mod disabled to confirm change
  - Test at 3-5 different rest areas to verify global application
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [ ] 8. Create troubleshooting documentation
  - Document common issues encountered during development
  - Write solutions for mod not appearing in mod manager
  - Write solutions for mod loading but radius unchanged
  - Document how to check game.log.txt for errors
  - Create guide for verifying folder structure and file paths
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 9. Document maintenance and extension procedures
  - Write guide for updating mod after ATS version updates
  - Document how to adjust radius values for different variants (1.5x, 2x, 3x)
  - Create instructions for adding DLC-specific rest area prefabs
  - Document best practices for testing with multiple mods
  - Write guide for sharing mod with other players
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [ ] 10. Create optional mod variants
  - Create 1.5x radius variant (radius: 22.5)
  - Create 2x radius variant (radius: 30.0)
  - Create 3x radius variant (radius: 45.0)
  - Package each variant as separate .scs file with descriptive names
  - Test each variant to ensure proper functionality
  - _Requirements: 8.3, 8.4_

- [ ] 11. Add DLC map support
  - Extract DLC map archives (if DLCs owned)
  - Identify DLC-specific rest area prefabs
  - Modify DLC rest area prefabs with increased radius
  - Add DLC prefab files to mod structure
  - Test on DLC maps to verify compatibility
  - _Requirements: 7.4, 8.1_
