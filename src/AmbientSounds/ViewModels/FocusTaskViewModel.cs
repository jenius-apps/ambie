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
        private bool _isCompleted;

        public FocusTaskViewModel(
            FocusTask task,
            IRelayCommand<FocusTaskViewModel> delete,
            IRelayCommand<FocusTaskViewModel> complete)
        {
            Guard.IsNotNull(task, nameof(task));
            Task = task;
            DeleteCommand = delete;
            CompleteCommand = complete;
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value) && value is true)
                {
                    CompleteCommand.Execute(this);
                }
            }
        }

        public FocusTask Task { get; }

        public IRelayCommand<FocusTaskViewModel> DeleteCommand { get; }

        public IRelayCommand<FocusTaskViewModel> CompleteCommand { get; }

        public string Text => Task.Text;
    }
}
