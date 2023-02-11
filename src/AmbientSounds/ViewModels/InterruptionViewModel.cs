using AmbientSounds.Models;
using Humanizer;
using Humanizer.Localisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class InterruptionViewModel
    {
        public InterruptionViewModel(FocusInterruption interruption, bool isLast = false)
        {
            MinutesInterrupted = TimeSpan.FromMinutes(interruption.Minutes).Humanize(maxUnit: TimeUnit.Minute);
            Notes = interruption.Notes;
            IsLast = isLast;
        }

        public string MinutesInterrupted { get; }

        public string Notes { get; }

        public bool IsLast { get; }
    }
}
