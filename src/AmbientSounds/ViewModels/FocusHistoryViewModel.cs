using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class FocusHistoryViewModel : ObservableObject
    {
        private readonly FocusHistory _focusHistory;

        public FocusHistoryViewModel(FocusHistory focusHistory)
        {
            Guard.IsNotNull(focusHistory, nameof(focusHistory));
            _focusHistory = focusHistory;
        }

        public FocusHistory FocusHistory => _focusHistory;
    }
}
