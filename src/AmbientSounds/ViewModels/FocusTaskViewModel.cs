using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskViewModel : ObservableObject
    {
        public FocusTaskViewModel(FocusTask task)
        {
            Guard.IsNotNull(task, nameof(task));
            Task = task;
        }

        public FocusTask Task { get; }

        public string Text => Task.Text;
    }
}
