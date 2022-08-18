using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AmbientSounds.Cache;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;

namespace AmbientSounds.Services
{
    public sealed class FocusHistoryService : IFocusHistoryService
    {
        private readonly IFocusHistoryCache _focusHistoryCache;
        private readonly IDialogService _dialogService;
        private readonly HashSet<string> _taskIdsCompleted = new();
        private FocusHistory? _activeHistory;

        public event EventHandler<FocusHistory?>? HistoryAdded;

        public FocusHistoryService(
            IFocusHistoryCache focusHistoryCache,
            IDialogService dialogService)
        {
            Guard.IsNotNull(focusHistoryCache, nameof(focusHistoryCache));
            Guard.IsNotNull(dialogService, nameof(dialogService));

            _focusHistoryCache = focusHistoryCache;
            _dialogService = dialogService;
        }

        public Task<IReadOnlyList<FocusHistory>> GetRecentAsync()
        {
            return _focusHistoryCache.GetRecentAsync();
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

        public async Task<(double, bool)> LogInterruptionAsync()
        {
            if (_activeHistory is null)
            {
                return (0, false);
            }

            (double minutes, string notes) = await _dialogService.OpenInterruptionAsync();

            if (minutes <= 0)
            {
                return (0, false);
            }

            _activeHistory.Interruptions.Add(new FocusInterruption
            {
                Minutes = minutes,
                Notes = notes,
                UtcTime = DateTime.UtcNow.Ticks
            });

            return (minutes, !string.IsNullOrWhiteSpace(notes));
        }

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
    }
}
