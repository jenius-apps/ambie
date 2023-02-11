using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusHistoryService
    {
        /// <summary>
        /// Event raised when a new history is added.
        /// </summary>
        event EventHandler<FocusHistory?>? HistoryAdded;

        /// <summary>
        /// Retrieves list of recent focus history.
        /// </summary>
        Task<IReadOnlyList<FocusHistory>> GetRecentAsync();

        /// <summary>
        /// Updates active history that a segment had ended.
        /// </summary>
        /// <param name="sessionType">The type of segment that just ended.</param>
        void TrackSegmentEnd(SessionType sessionType);

        /// <summary>
        /// Closes the current active history and saves to disk.
        /// </summary>
        /// <param name="ticks">Utc ticks when the focus session completed.</param>
        void TrackHistoryCompletion(long utcTicks, SessionType lastCompletedSegmentType);

        /// <summary>
        /// Starts a new history to track. Note: any previous active history
        /// will be abandoned. (There shouldn't be an active history unless
        /// this service's sequence of operations was incorrectly called).
        /// </summary>
        /// <param name="utcTicks">The Utc ticks for the start time.</param>
        /// <param name="focusLength">Length of each focus segment.</param>
        /// <param name="restLength">Length of each rest segment.</param>
        /// <param name="repetitions">Number of repetitions.</param>
        void TrackNewHistory(long utcTicks, int focusLength, int restLength, int repetitions);

        /// <summary>
        /// Closes the current active history while being incomplete. 
        /// </summary>
        void TrackIncompleteHistory(long utcTicks, SessionType partialSegmentType, TimeSpan minutesUsedInIncompleteSegment);

        /// <summary>
        /// Logs an interruption using the given data.
        /// </summary>
        void LogInterruption(double minutes, string notes);

        /// <summary>
        /// Logs that a task was completed during a focus session.
        /// If there is no active session, no operation is performed.
        /// </summary>
        void LogTaskCompleted(string taskId);

        /// <summary>
        /// Removes a task that was previously logged as completed.
        /// </summary>
        void RevertTaskCompleted(string taskId);

        /// <summary>
        /// Returns the number of interruptions in the currently
        /// active focus session history. Returns 0 if there
        /// are no active sessions.
        /// </summary>
        int GetInterruptionCount();

        /// <summary>
        /// Returns the start time of the active focus session,
        /// if one is available. Otherwise, returns DateTime minimum.
        /// </summary>
        /// <returns></returns>
        DateTime GetStartTime();
        void LogPause();
        int GetPauses();

        /// <summary>
        /// Retrieves the recent list of
        /// interruptions.
        /// </summary>
        /// <returns>List of recent interruptions.</returns>
        Task<IReadOnlyList<FocusInterruption>> GetRecentInterruptionsAsync();
        
        /// <summary>
        /// Prepares a dictionary of data
        /// to be used for telemetry.
        /// </summary>
        Dictionary<string, string> GatherInterruptionTelemetry(double minutes, string notes, bool isCompact);
    }
}
