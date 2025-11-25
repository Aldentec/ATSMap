@echo off
echo ============================================
echo   ATS Telemetry Plugin Checker
echo ============================================
echo.

set "DOCS=%USERPROFILE%\Documents"
set "ATS_PLUGINS=%DOCS%\American Truck Simulator\bin\win_x64\plugins"
set "ATS_LOG=%DOCS%\American Truck Simulator\game.log.txt"

echo Checking for telemetry plugin installation...
echo.
echo Expected plugin location:
echo %ATS_PLUGINS%
echo.

if exist "%ATS_PLUGINS%" (
    echo [OK] Plugins folder exists
    echo.
    echo Files in plugins folder:
    dir /b "%ATS_PLUGINS%"
    echo.
) else (
    echo [ERROR] Plugins folder does NOT exist!
    echo You need to create it: %ATS_PLUGINS%
    echo.
)

echo.
echo Checking for game log file...
if exist "%ATS_LOG%" (
    echo [OK] Game log found
    echo.
    echo Last 30 lines of game.log.txt:
    echo ----------------------------------------
    powershell -Command "Get-Content '%ATS_LOG%' -Tail 30"
    echo ----------------------------------------
) else (
    echo [WARNING] Game log not found at: %ATS_LOG%
    echo Make sure ATS has been run at least once.
)

echo.
echo.
echo ============================================
echo   What to look for:
echo ============================================
echo 1. Plugin DLL should be in plugins folder
echo 2. Game log should show plugin loading
echo 3. Look for lines mentioning "telemetry" or "plugin"
echo.
pause
