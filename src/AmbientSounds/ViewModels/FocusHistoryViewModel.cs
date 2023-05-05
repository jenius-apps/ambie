using AmbientSounds.Extensions;
using AmbientSounds.Models;
using Humanizer;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace AmbientSounds.ViewModels
{
    public class FocusHistoryViewModel : ObservableObject
    {
        private readonly DateTime _localStart;
        private readonly DateTime _localEnd;

        public FocusHistoryViewModel(FocusHistory focusHistory)
        {
            Guard.IsNotNull(focusHistory, nameof(focusHistory));

            var percent = focusHistory.GetPercentComplete();
            PercentComplete = percent >= 100
                ? $"{percent}%"
                : $"{percent:N1}%";

            //Award = focusHistory.GetAward(percent);
            _localStart = new DateTime(focusHistory.StartUtcTicks, DateTimeKind.Utc).ToLocalTime();
            _localEnd = new DateTime(focusHistory.EndUtcTicks, DateTimeKind.Utc).ToLocalTime();

            int rounds = focusHistory.Repetitions + 1;
            FocusMinutes = Math.Round(focusHistory.GetFocusTimeCompleted(), 1);
            FocusInfo = $"{focusHistory.FocusSegmentsCompleted}/{rounds}";
            TotalFocusMinutes = focusHistory.GetFocusTimeTotal();
            FocusMinutesFraction = $"{FocusMinutes}/{TotalFocusMinutes}";

            RestMinutes = Math.Round(focusHistory.GetRestTimeCompleted(), 1);

            InterruptionCount = focusHistory.Interruptions.Count;
            TasksCompleted = focusHistory.TasksCompleted;

            for (int i = 0; i < focusHistory.Interruptions.Count; i++)
            {
                Interruptions.Add(new InterruptionViewModel(
                    focusHistory.Interruptions[i],
                    i == focusHistory.Interruptions.Count - 1));
            }
        }

        public string PercentComplete { get; }

        //public HistoryAward Award { get; }

        /// <summary>
        /// Human-friendly time string such as '3 days ago'.
        /// </summary>
        /// <remarks>
        /// The lack of backing field is by design.
        /// Each time the UI retrieves this value,
        /// We want the string to be updated.
        /// </remarks>
        public string TimeElapsed => _localStart.Humanize();

        public string StartTime => _localStart.ToShortTimeString();

        public string EndTime => _localEnd.ToShortTimeString();

        public double FocusMinutes { get; }

        public double TotalFocusMinutes { get; }

        public string FocusMinutesFraction { get; }

        public double RestMinutes { get; }

        public string FocusInfo { get; }

        public int InterruptionCount { get; }

        public int TasksCompleted { get; }

        public List<InterruptionViewModel> Interruptions { get; } = new();

        public override string ToString()
        {
            return _localStart.ToLongDateString();
        }
    }
}
