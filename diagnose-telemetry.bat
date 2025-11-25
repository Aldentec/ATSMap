@echo off
echo ============================================
echo   ATS Telemetry Diagnostic Tool
echo ============================================
echo.

set "DOCS=%USERPROFILE%\Documents"
set "ATS_PLUGINS=%DOCS%\American Truck Simulator\bin\win_x64\plugins"

echo Step 1: Checking plugin folder...
echo ========================================
echo.
echo Expected location: %ATS_PLUGINS%
echo.

if exist "%ATS_PLUGINS%" (
    echo [OK] Plugins folder exists
    echo.
    echo Files in plugins folder:
    dir /b "%ATS_PLUGINS%" 2>nul
    if errorlevel 1 (
        echo   (folder is empty)
    )
) else (
    echo [ERROR] Plugins folder does NOT exist!
    echo.
    echo Creating folder...
    mkdir "%ATS_PLUGINS%" 2>nul
    if exist "%ATS_PLUGINS%" (
        echo [OK] Folder created successfully
    ) else (
        echo [ERROR] Could not create folder
    )
)

echo.
echo.
echo Step 2: Checking shared memory...
echo ========================================
echo.
dotnet run --project src\ATSLiveMap.TestConsole\ATSLiveMap.TestConsole.csproj -- --check

echo.
echo.
echo ============================================
echo   What to do next:
echo ============================================
echo.
echo If NO shared memory was found:
echo   1. Download telemetry plugin from:
echo      https://github.com/Funbit/ets2-telemetry-server/releases
echo   2. Run the installer (easiest option)
echo      OR copy the DLL to: %ATS_PLUGINS%
echo   3. Restart ATS
echo   4. Load a game (must be in-game, not menu)
echo   5. Run this diagnostic again
echo.
pause
