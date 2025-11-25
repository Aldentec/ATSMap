# List all memory-mapped files (shared memory regions)
Write-Host "Searching for all shared memory regions..." -ForegroundColor Cyan
Write-Host ""

# Common telemetry-related names to check
$names = @(
    "Local\SCSTelemetry",
    "Local\SimTelemetryETS2", 
    "Local\SimTelemetryATS",
    "SCSTelemetry",
    "Local\SHETS2Telemetry",
    "SHETS2Telemetry",
    "Local\TruckyTelemetry",
    "TruckyTelemetry",
    "Local\ETS2",
    "Local\ATS"
)

$found = $false

foreach ($name in $names) {
    try {
        $mmf = [System.IO.MemoryMappedFiles.MemoryMappedFile]::OpenExisting($name)
        Write-Host "[FOUND] $name" -ForegroundColor Green
        $mmf.Dispose()
        $found = $true
    }
    catch {
        # Not found, skip
    }
}

if (-not $found) {
    Write-Host "No telemetry shared memory regions found." -ForegroundColor Red
    Write-Host ""
    Write-Host "The SHETS2Telemetry plugin is loaded but may use a different method."
    Write-Host "Try installing the Funbit telemetry server instead:"
    Write-Host "https://github.com/Funbit/ets2-telemetry-server/releases"
}
