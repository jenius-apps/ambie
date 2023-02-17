using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;

namespace AmbientSounds.Services
{
    public sealed class FocusHistoryService : IFocusHistoryService
    {
        private readonly IFocusHistoryCache _focusHistoryCache;
        private readonly HashSet<string> _taskIdsCompleted = new();
        private FocusHistory? _activeHistory;

        public event EventHandler<FocusHistory?>? HistoryAdded;

        public FocusHistoryService(
            IFocusHistoryCache focusHistoryCache)
        {
            Guard.IsNotNull(focusHistoryCache, nameof(focusHistoryCache));

            _focusHistoryCache = focusHistoryCache;
        }

        public int GetInterruptionCount()
        {
            return _activeHistory?.Interruptions.Count ?? 0;
        }

        public DateTime GetStartTime()
        {
            var startTime = new DateTime(_activeHistory?.StartUtcTicks ?? 0, DateTimeKind.Utc);
            return startTime.ToLocalTime();
        }

        public Task<IReadOnlyList<FocusHistory>> GetRecentAsync()
        {
            return _focusHistoryCache.GetRecentAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<FocusInterruption>> GetRecentInterruptionsAsync()
        {
            var histories = await GetRecentAsync();

            if (histories.OrderByDescending(x => x.StartUtcTicks).FirstOrDefault() is { Interruptions.Count: > 0 })
            {
                List<FocusInterruption> results = new();
                foreach (var h in histories)
                {
                    results.AddRange(h.Interruptions);
                }

                if (results.Count >= 3)
                {
                    return results;
                }
            }

            return Array.Empty<FocusInterruption>();
        }

        public void TrackHistoryCompletion(long utcTicks, SessionType lastCompletedSegmentType)
        {
            if (_activeHistory is null)
            {
                return;
            }

            TrackSegmentEnd(lastCompletedSegmentType);
            _activeHistory.EndUtcTicks = utcTicks;
            _activeHistory.TasksCompleted = _taskIdsCompleted.Count;

            _ = _focusHistoryCache.AddHistoryAsync(_activeHistory);

            HistoryAdded?.Invoke(this, _activeHistory);
            _activeHistory = null;
            _taskIdsCompleted.Clear();
        }

        public void TrackIncompleteHistory(
            long utcTicks,
            SessionType partialSegmentType,
            TimeSpan minutesUsedInIncompleteSegment)
        {
            if (_activeHistory is null)
            {
                return;
            }

            _activeHistory.PartialSegmentType = partialSegmentType;
            _activeHistory.PartialSegmentTicks = minutesUsedInIncompleteSegment.Ticks;
            _activeHistory.EndUtcTicks = utcTicks;
            _activeHistory.TasksCompleted = _taskIdsCompleted.Count;

            _ = _focusHistoryCache.AddHistoryAsync(_activeHistory);

            HistoryAdded?.Invoke(this, _activeHistory);
            _activeHistory = null;
            _taskIdsCompleted.Clear();
        }

        public void TrackNewHistory(long utcTicks, int focusLength, int restLength, int repetitions)
        {
            _activeHistory = new FocusHistory
            {
                StartUtcTicks = utcTicks,
                FocusLength = focusLength,
                RestLength = restLength,
                Repetitions = repetitions
            };
        }

        public void TrackSegmentEnd(SessionType sessionType)
        {
            if (_activeHistory is null)
            {
                return;
            }

            if (sessionType is SessionType.Focus)
            {
                _activeHistory.FocusSegmentsCompleted++;
            }
            else if (sessionType is SessionType.Rest)
            {
                _activeHistory.RestSegmentsCompleted++;
            }
        }

        /// <inheritdoc/>
        public void LogInterruption(double minutes, string notes)
        {
            if (_activeHistory is null || minutes <= 0)
            {
                return;
            }

            _activeHistory.Interruptions.Add(new FocusInterruption
            {
                Minutes = minutes,
                Notes = notes.Trim(),
                UtcTime = DateTime.UtcNow.Ticks
            });
        }

        public void LogPause()
        {
            if (_activeHistory is FocusHistory f)
            {
                f.Pauses++;
            }
        }

        public int GetPauses() => _activeHistory?.Pauses ?? 0;

        public void LogTaskCompleted(string taskId)
        {
            if (_activeHistory is null)
            {
                // If there's no active session, don't log it.
                return;
            }

            _taskIdsCompleted.Add(taskId);
        }

        public void RevertTaskCompleted(string taskId)
        {
            _taskIdsCompleted.Remove(taskId);
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GatherInterruptionTelemetry(
            double minutes,
            string notes,
            bool isCompact)
        {
            bool hasNotes = !string.IsNullOrWhiteSpace(notes);
            var data = new Dictionary<string, string>
            {
                { "minutes", minutes.ToString() },
                { "hasNotes", hasNotes.ToString().ToLower() },
                { "isCompact", isCompact.ToString().ToLower() }
            };

            if (hasNotes)
            {
                data.Add("notes", notes);
            }

            return data;
        }
    }
}
