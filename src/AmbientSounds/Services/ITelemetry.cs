using System.Collections.Generic;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for a telemetry client.
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// Tracks the given event and its properties.
        /// </summary>
        /// <param name="eventName">Name of event.</param>
        /// <param name="properties">Optoinal properties associated with the event.</param>
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null);
    }
}
