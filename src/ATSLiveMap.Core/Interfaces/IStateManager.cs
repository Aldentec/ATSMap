using System;
using ATSLiveMap.Core.Models;

namespace ATSLiveMap.Core.Interfaces
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public interface IStateManager
    {
        // Current player state
        PlayerState CurrentState { get; }

        // Connection status
        ConnectionStatus Status { get; }

        // Event fired when state updates (like EventEmitter)
        event EventHandler<PlayerState> StateUpdated;

        // Update state from new telemetry data
        void UpdateFromTelemetry(TelemetryData data);
    }
}
