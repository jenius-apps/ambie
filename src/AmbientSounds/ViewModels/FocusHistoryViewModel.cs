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
        private readonly FocusHistory _focusHistory;

        public FocusHistoryViewModel(FocusHistory focusHistory)
        {
            Guard.IsNotNull(focusHistory, nameof(focusHistory));
            _focusHistory = focusHistory;

            var percent = focusHistory.GetPercentComplete();
            PercentComplete = percent >= 100
                ? $"{percent}%"
                : $"{percent:N1}%";

            Award = focusHistory.GetAward(percent);
            TimeElapsed = new DateTime(focusHistory.StartUtcTicks, DateTimeKind.Utc).ToLocalTime().Humanize();
        }

        public FocusHistory FocusHistory => _focusHistory;

        public string PercentComplete { get; }

        public HistoryAward Award { get; }

        public string TimeElapsed { get; }
    }
}
