@echo off
cls
echo ============================================
echo   ATS Live Map - Quick Diagnostic
echo ============================================
echo.
echo Running diagnostic to check telemetry setup...
echo.

dotnet build src\ATSLiveMap.TestConsole\ATSLiveMap.TestConsole.csproj --nologo --verbosity quiet

echo.
dotnet run --project src\ATSLiveMap.TestConsole\ATSLiveMap.TestConsole.csproj --no-build -- --check

echo.
echo.
echo ============================================
echo   Next Steps:
echo ============================================
echo.
echo If shared memory was found:
echo   - Run: run-telemetry-test.bat
echo   - You should see live telemetry data!
echo.
echo If NO shared memory was found:
echo   - Read: TELEMETRY-SETUP.md
echo   - Install the telemetry plugin
echo   - Run this diagnostic again
echo.
pause
