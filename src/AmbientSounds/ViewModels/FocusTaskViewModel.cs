using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskViewModel : ObservableObject
    {
        public FocusTaskViewModel(
            FocusTask task,
            IRelayCommand<FocusTaskViewModel> delete)
        {
            Guard.IsNotNull(task, nameof(task));
            Task = task;
            DeleteCommand = delete;
        }

        public FocusTask Task { get; }

        public IRelayCommand<FocusTaskViewModel> DeleteCommand { get; }

        public string Text => Task.Text;
    }
}
