using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;  // For Application.Current.Dispatcher
using ProHauler.Core.Interfaces;

namespace ProHauler.Telemetry
{
    /// <summary>
    /// Polls telemetry data from a client and updates the state manager on the UI thread.
    /// Handles thread marshaling to ensure state updates occur on the WPF dispatcher thread.
    /// </summary>
    public class TelemetryPoller
    {
        private readonly ITelemetryClient _client;
        private readonly IStateManager _stateManager;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Initializes a new instance of the TelemetryPoller class.
        /// </summary>
        /// <param name="client">The telemetry client to poll data from.</param>
        /// <param name="stateManager">The state manager to update with telemetry data.</param>
        public TelemetryPoller(ITelemetryClient client, IStateManager stateManager)
        {
            _client = client;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Starts the polling loop on a background thread.
        /// Polls telemetry data and marshals updates to the UI thread.
        /// </summary>
        /// <returns>A task representing the asynchronous polling operation.</returns>
        public async Task StartAsync()
        {
            // Create cancellation token (like AbortController in fetch API)
            _cts = new CancellationTokenSource();

            // Run on background thread (like Web Worker)
            await Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Get telemetry data on background thread
                        var data = await _client.GetCurrentDataAsync();

                        // Marshal to UI thread (IMPORTANT!)
                        // This is like postMessage to main thread in Web Workers
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            // This code runs on UI thread
                            _stateManager.UpdateFromTelemetry(data);
                        });

                        // Wait 50ms before next poll (20 Hz)
                        await Task.Delay(50, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal cancellation, exit loop
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Polling error: {ex.Message}");
                        // Continue polling despite errors
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Stops the polling loop and cancels any pending operations.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
