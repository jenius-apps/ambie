using AmbientSounds.Extensions;
using AmbientSounds.Models;
using Humanizer;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels
{
    public class FocusHistoryViewModel : ObservableObject
    {
        private readonly DateTime _localStart;

        public FocusHistoryViewModel(FocusHistory focusHistory)
        {
            Guard.IsNotNull(focusHistory, nameof(focusHistory));

            var percent = focusHistory.GetPercentComplete();
            PercentComplete = percent >= 100
                ? $"{percent}%"
                : $"{percent:N1}%";

            //Award = focusHistory.GetAward(percent);
            _localStart = new DateTime(focusHistory.StartUtcTicks, DateTimeKind.Utc).ToLocalTime();

            int rounds = focusHistory.Repetitions + 1;
            FocusInfo = $"{focusHistory.GetFocusTimeCompleted():N1}/{focusHistory.FocusLength * rounds}";
            RestInfo = $"{focusHistory.GetRestTimeCompleted():N1}/{focusHistory.RestLength * rounds}";
            InterruptionCount = focusHistory.Interruptions.Count;
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

        public string FocusInfo { get; }

        public string RestInfo { get; }

        public int InterruptionCount { get; }
    }
}
