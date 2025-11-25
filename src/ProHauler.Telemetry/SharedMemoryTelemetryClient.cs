using System;
using System.IO;
using System.IO.MemoryMappedFiles;  // For shared memory access
using System.Text;
using System.Text.Json;  // For JSON parsing
using System.Threading;
using System.Threading.Tasks;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Telemetry
{
    /// <summary>
    /// Telemetry client that reads game data from shared memory using the SCS SDK plugin.
    /// Supports multiple shared memory naming conventions for compatibility with different plugins.
    /// </summary>
    public class SharedMemoryTelemetryClient : ITelemetryClient, IDisposable
    {
        // Try multiple shared memory names (different plugins use different names)
        private static readonly string[] MemoryMapNames = new[]
        {
            "Local\\SCSTelemetry",      // Standard SCS SDK
            "Local\\SimTelemetryETS2",  // Funbit telemetry server for ETS2
            "Local\\SimTelemetryATS",   // Funbit telemetry server for ATS
            "SCSTelemetry"              // Without Local prefix
        };
        private const int PollingIntervalMs = 50;  // 20 updates per second
        private string? _activeMemoryMapName;

        private MemoryMappedFile? _memoryMappedFile;
        private Timer? _pollingTimer;
        private TelemetryData _lastData = new();
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private int _reconnectDelayMs = 1000;

        /// <inheritdoc/>
        public bool IsConnected => _status == ConnectionStatus.Connected;

        /// <inheritdoc/>
        public event EventHandler<TelemetryData>? DataUpdated;

        /// <inheritdoc/>
        public event EventHandler<string>? ConnectionStatusChanged;

        /// <inheritdoc/>
        public TelemetryData GetCurrentData()
        {
            return _lastData;
        }

        /// <inheritdoc/>
        public async Task<TelemetryData> GetCurrentDataAsync()
        {
            return await Task.Run(() => _lastData);
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            // Infinite loop with retry logic
            while (true)
            {
                try
                {
                    _status = ConnectionStatus.Connecting;
                    OnConnectionStatusChanged("Connecting to ATS...");

                    // Try each possible shared memory name
                    bool connected = false;
                    foreach (var memoryMapName in MemoryMapNames)
                    {
                        try
                        {
                            // Try to open the shared memory region
                            // This will throw FileNotFoundException if it doesn't exist
                            _memoryMappedFile = MemoryMappedFile.OpenExisting(memoryMapName);
                            _activeMemoryMapName = memoryMapName;
                            connected = true;
                            OnConnectionStatusChanged($"Connected to ATS (using {memoryMapName})");
                            break;
                        }
                        catch (FileNotFoundException)
                        {
                            // This memory map doesn't exist, try next one
                            continue;
                        }
                    }

                    if (!connected)
                    {
                        throw new FileNotFoundException("No telemetry shared memory found");
                    }

                    _status = ConnectionStatus.Connected;
                    _reconnectDelayMs = 1000;  // Reset delay on success

                    // Start polling timer (like setInterval in JS)
                    _pollingTimer = new Timer(
                        PollTelemetry,           // Callback function
                        null,                    // State (not used)
                        0,                       // Start immediately
                        PollingIntervalMs        // Repeat every 50ms
                    );

                    break;  // Exit retry loop on success
                }
                catch (FileNotFoundException)
                {
                    // ATS not running or plugin not loaded
                    _status = ConnectionStatus.Disconnected;
                    OnConnectionStatusChanged("Waiting for ATS to start...");

                    // Wait before retrying (exponential backoff)
                    await Task.Delay(_reconnectDelayMs);

                    // Increase delay for next retry (max 5 seconds)
                    _reconnectDelayMs = Math.Min(_reconnectDelayMs * 2, 5000);
                }
                catch (Exception ex)
                {
                    _status = ConnectionStatus.Error;
                    OnConnectionStatusChanged($"Error: {ex.Message}");
                    await Task.Delay(_reconnectDelayMs);
                }
            }
        }

        /// <inheritdoc/>
        public Task StopAsync()
        {
            _pollingTimer?.Dispose();
            _memoryMappedFile?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Polls the shared memory for new telemetry data.
        /// Called by the timer every 50ms.
        /// </summary>
        /// <param name="state">Timer state (not used).</param>
        private void PollTelemetry(object? state)
        {
            try
            {
                if (_memoryMappedFile == null) return;

                // Create a view accessor to read from shared memory
                using (var accessor = _memoryMappedFile.CreateViewAccessor(
                    0,                              // Start at beginning
                    0,                              // Read entire region
                    MemoryMappedFileAccess.Read))   // Read-only access
                {
                    // Read bytes from memory
                    byte[] buffer = new byte[accessor.Capacity];
                    accessor.ReadArray(0, buffer, 0, buffer.Length);

                    // Convert bytes to string (like Buffer.toString() in Node.js)
                    string json = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                    // Parse JSON (like JSON.parse())
                    var data = ParseTelemetryJson(json);

                    if (data != null)
                    {
                        _lastData = data;
                        OnDataUpdated(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _status = ConnectionStatus.Error;
                OnConnectionStatusChanged($"Read error: {ex.Message}");

                // Attempt reconnection
                _pollingTimer?.Dispose();
                _ = StartAsync();  // Fire and forget (like not awaiting a promise)
            }
        }

        /// <summary>
        /// Parses JSON data from shared memory into a TelemetryData object.
        /// </summary>
        /// <param name="json">The JSON string from shared memory.</param>
        /// <returns>A TelemetryData object, or null if parsing fails.</returns>
        private TelemetryData? ParseTelemetryJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                var truck = root.GetProperty("truck");
                var position = truck.GetProperty("position");
                var orientation = truck.GetProperty("orientation");
                var game = root.GetProperty("game");

                return new TelemetryData
                {
                    IsConnected = game.GetProperty("connected").GetBoolean(),
                    GameName = game.GetProperty("gameName").GetString() ?? "",
                    IsPaused = game.GetProperty("paused").GetBoolean(),

                    Position = new Vector3(
                        position.GetProperty("X").GetSingle(),
                        position.GetProperty("Y").GetSingle(),
                        position.GetProperty("Z").GetSingle()
                    ),

                    Heading = orientation.GetProperty("heading").GetSingle(),
                    Pitch = orientation.GetProperty("pitch").GetSingle(),
                    Roll = orientation.GetProperty("roll").GetSingle(),
                    Speed = truck.GetProperty("speed").GetSingle(),

                    Timestamp = DateTime.Now
                };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parse error: {ex.Message}");
                return null;
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Missing JSON property: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Raises the DataUpdated event.
        /// </summary>
        /// <param name="data">The updated telemetry data.</param>
        protected virtual void OnDataUpdated(TelemetryData data)
        {
            DataUpdated?.Invoke(this, data);
        }

        /// <summary>
        /// Raises the ConnectionStatusChanged event.
        /// </summary>
        /// <param name="message">The status message.</param>
        protected virtual void OnConnectionStatusChanged(string message)
        {
            ConnectionStatusChanged?.Invoke(this, message);
        }

        /// <summary>
        /// Disposes resources used by the shared memory telemetry client.
        /// </summary>
        public void Dispose()
        {
            _pollingTimer?.Dispose();
            _memoryMappedFile?.Dispose();
        }
    }
}
