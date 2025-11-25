@echo off
echo ============================================
echo   ATS Live Map - Telemetry Test Console
echo ============================================
echo.
echo Building test console...
dotnet build src\ATSLiveMap.TestConsole\ATSLiveMap.TestConsole.csproj --nologo --verbosity quiet
echo.
echo Starting test console...
echo.
dotnet run --project src\ATSLiveMap.TestConsole\ATSLiveMap.TestConsole.csproj --no-build
