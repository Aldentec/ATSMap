using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Telemetry
{
    /// <summary>
    /// Telemetry client that connects to Funbit's ETS2 Telemetry Server via HTTP.
    /// The server runs on http://localhost:25555 by default and provides game data through a REST API.
    /// </summary>
    public class HttpTelemetryClient : ITelemetryClient, IDisposable
    {
        private const string DefaultServerUrl = "http://localhost:25555/api/ets2/telemetry";
        private const int PollingIntervalMs = 100;  // 10 updates per second (HTTP is slower)

        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private Timer? _pollingTimer;
        private TelemetryData _lastData = new();
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private bool _isRunning;

        /// <inheritdoc/>
        public bool IsConnected => _status == ConnectionStatus.Connected;

        /// <inheritdoc/>
        public event EventHandler<TelemetryData>? DataUpdated;

        /// <inheritdoc/>
        public event EventHandler<string>? ConnectionStatusChanged;

        /// <summary>
        /// Initializes a new instance of the HttpTelemetryClient class.
        /// </summary>
        /// <param name="serverUrl">The URL of the telemetry server. If null, uses the default URL.</param>
        public HttpTelemetryClient(string? serverUrl = null)
        {
            _serverUrl = serverUrl ?? DefaultServerUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
        }

        /// <inheritdoc/>
        public TelemetryData GetCurrentData()
        {
            return _lastData;
        }

        /// <inheritdoc/>
        public async Task<TelemetryData> GetCurrentDataAsync()
        {
            return await Task.FromResult(_lastData);
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            _isRunning = true;
            _status = ConnectionStatus.Connecting;
            OnConnectionStatusChanged("Connecting to telemetry server...");

            // Start polling timer
            _pollingTimer = new Timer(
                async _ => await PollTelemetryAsync(),
                null,
                0,
                PollingIntervalMs
            );

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync()
        {
            _isRunning = false;
            _pollingTimer?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Polls the telemetry server for new data via HTTP GET request.
        /// </summary>
        private async Task PollTelemetryAsync()
        {
            if (!_isRunning) return;

            try
            {
                var response = await _httpClient.GetStringAsync(_serverUrl);
                var data = ParseTelemetryJson(response);

                if (data != null)
                {
                    // Update connection status
                    if (_status != ConnectionStatus.Connected)
                    {
                        _status = ConnectionStatus.Connected;
                        OnConnectionStatusChanged($"Connected to telemetry server at {_serverUrl}");
                    }

                    _lastData = data;
                    OnDataUpdated(data);
                }
            }
            catch (HttpRequestException ex)
            {
                if (_status != ConnectionStatus.Disconnected)
                {
                    _status = ConnectionStatus.Disconnected;
                    OnConnectionStatusChanged($"Waiting for telemetry server... ({ex.Message})");
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout - server not responding
                if (_status != ConnectionStatus.Disconnected)
                {
                    _status = ConnectionStatus.Disconnected;
                    OnConnectionStatusChanged("Telemetry server timeout");
                }
            }
            catch (Exception ex)
            {
                if (_status != ConnectionStatus.Error)
                {
                    _status = ConnectionStatus.Error;
                    OnConnectionStatusChanged($"Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Parses JSON response from the telemetry server into a TelemetryData object.
        /// </summary>
        /// <param name="json">The JSON string from the server.</param>
        /// <returns>A TelemetryData object, or null if parsing fails.</returns>
        private TelemetryData? ParseTelemetryJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Funbit telemetry server JSON structure
                var game = root.GetProperty("game");
                var truck = root.GetProperty("truck");

                // Check if connected
                bool isConnected = game.GetProperty("connected").GetBoolean();
                if (!isConnected)
                {
                    return null;
                }

                // Get placement (contains position and orientation)
                var placement = truck.GetProperty("placement");
                float x = placement.GetProperty("x").GetSingle();
                float y = placement.GetProperty("y").GetSingle();
                float z = placement.GetProperty("z").GetSingle();
                float heading = placement.GetProperty("heading").GetSingle();
                float pitch = placement.GetProperty("pitch").GetSingle();
                float roll = placement.GetProperty("roll").GetSingle();

                // Get speed (already in m/s in Funbit API)
                float speedMs = truck.GetProperty("speed").GetSingle();

                // Get performance tracking fields with graceful fallback
                float fuelAmount = 0f;
                float fuelCapacity = 0f;
                float damagePercent = 0f;
                float odometer = 0f;
                float speedLimit = 0f;

                try
                {
                    if (truck.TryGetProperty("fuel", out JsonElement fuel))
                    {
                        if (fuel.TryGetProperty("value", out JsonElement fuelValue))
                            fuelAmount = fuelValue.GetSingle();
                        if (fuel.TryGetProperty("capacity", out JsonElement fuelCap))
                            fuelCapacity = fuelCap.GetSingle();
                    }
                }
                catch { /* Use default value */ }

                try
                {
                    // Damage is reported as "wear" fields (0-1 range)
                    float wearEngine = truck.TryGetProperty("wearEngine", out var we) ? we.GetSingle() : 0f;
                    float wearTransmission = truck.TryGetProperty("wearTransmission", out var wt) ? wt.GetSingle() : 0f;
                    float wearCabin = truck.TryGetProperty("wearCabin", out var wc) ? wc.GetSingle() : 0f;
                    float wearChassis = truck.TryGetProperty("wearChassis", out var wch) ? wch.GetSingle() : 0f;
                    float wearWheels = truck.TryGetProperty("wearWheels", out var ww) ? ww.GetSingle() : 0f;

                    // Average the wear components and convert to percentage
                    damagePercent = ((wearEngine + wearTransmission + wearCabin + wearChassis + wearWheels) / 5.0f) * 100f;
                }
                catch { /* Use default value */ }

                try
                {
                    if (truck.TryGetProperty("odometer", out JsonElement odometerValue))
                        odometer = odometerValue.GetSingle();
                }
                catch { /* Use default value */ }

                try
                {
                    if (root.TryGetProperty("navigation", out JsonElement navigation))
                    {
                        if (navigation.TryGetProperty("speedLimit", out JsonElement speedLimitValue))
                        {
                            float limit = speedLimitValue.GetSingle();
                            // Convert from m/s to MPH if needed, or use as-is
                            speedLimit = limit;
                        }
                    }
                }
                catch { /* Use default value */ }

                // Parse safety and driving behavior fields
                bool blinkerLeftActive = truck.TryGetProperty("blinkerLeftActive", out var bla) && bla.GetBoolean();
                bool blinkerRightActive = truck.TryGetProperty("blinkerRightActive", out var bra) && bra.GetBoolean();
                bool lightsBeamHighOn = truck.TryGetProperty("lightsBeamHighOn", out var lbh) && lbh.GetBoolean();
                bool parkBrakeOn = truck.TryGetProperty("parkBrakeOn", out var pbo) && pbo.GetBoolean();
                bool motorBrakeOn = truck.TryGetProperty("motorBrakeOn", out var mbo) && mbo.GetBoolean();
                int retarderBrake = truck.TryGetProperty("retarderBrake", out var rb) ? rb.GetInt32() : 0;
                bool cruiseControlOn = truck.TryGetProperty("cruiseControlOn", out var cco) && cco.GetBoolean();
                float engineRpm = truck.TryGetProperty("engineRpm", out var erpm) ? erpm.GetSingle() : 0f;
                float engineRpmMax = truck.TryGetProperty("engineRpmMax", out var erpmMax) ? erpmMax.GetSingle() : 2000f;
                float brakeTemp = truck.TryGetProperty("brakeTemperature", out var bt) ? bt.GetSingle() : 0f;

                return new TelemetryData
                {
                    IsConnected = isConnected,
                    GameName = game.GetProperty("gameName").GetString() ?? "Unknown",
                    IsPaused = game.GetProperty("paused").GetBoolean(),
                    Position = new Vector3(x, y, z),
                    Heading = heading,
                    Pitch = pitch,
                    Roll = roll,
                    Speed = speedMs,
                    Timestamp = DateTime.Now,
                    FuelAmount = fuelAmount,
                    FuelCapacity = fuelCapacity,
                    DamagePercent = damagePercent,
                    Odometer = odometer,
                    SpeedLimit = speedLimit,
                    BlinkerLeftActive = blinkerLeftActive,
                    BlinkerRightActive = blinkerRightActive,
                    LightsBeamHighOn = lightsBeamHighOn,
                    ParkBrakeOn = parkBrakeOn,
                    MotorBrakeOn = motorBrakeOn,
                    RetarderBrake = retarderBrake,
                    CruiseControlOn = cruiseControlOn,
                    EngineRpm = engineRpm,
                    EngineRpmMax = engineRpmMax,
                    BrakeTemperature = brakeTemp
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON parse error: {ex.Message}");
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
        /// Disposes resources used by the HTTP telemetry client.
        /// </summary>
        public void Dispose()
        {
            _pollingTimer?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
