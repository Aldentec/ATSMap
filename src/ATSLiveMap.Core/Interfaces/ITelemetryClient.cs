using System;
using System.Threading.Tasks;
using ATSLiveMap.Core.Models;

namespace ATSLiveMap.Core.Interfaces
{
    // interface defines a contract (like TypeScript interface)
    // By convention, interface names start with 'I'
    public interface ITelemetryClient
    {
        // Properties (like interface properties in TS)
        bool IsConnected { get; }

        // Methods (like interface methods in TS)
        TelemetryData GetCurrentData();

        // Task<T> is like Promise<T> in JS
        Task<TelemetryData> GetCurrentDataAsync();

        // Events are like EventEmitter in Node.js
        event EventHandler<TelemetryData> DataUpdated;
        event EventHandler<string> ConnectionStatusChanged;

        // Async methods for starting/stopping
        Task StartAsync();
        Task StopAsync();
    }
}
