using AmbientSounds.Models;
using Humanizer;
using Humanizer.Localisation;
using System;

namespace AmbientSounds.ViewModels
{
    public class InterruptionViewModel
    {
        public InterruptionViewModel(FocusInterruption interruption, bool isLast = false)
        {
            MinutesInterrupted = TimeSpan.FromMinutes(interruption.Minutes).Humanize(maxUnit: TimeUnit.Minute);
            Notes = interruption.Notes;
            IsLast = isLast;
            DateTimeString = new DateTime(interruption.UtcTime, DateTimeKind.Utc)
                .ToLocalTime()
                .ToLongDateString();
        }

        public string MinutesInterrupted { get; }

        public string Notes { get; }

        public bool IsLast { get; }

        public string DateTimeString { get; }
    }
}
