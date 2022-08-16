using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskViewModel : ObservableObject
    {
        private bool _isCompleted;

        public FocusTaskViewModel(
            FocusTask task,
            IRelayCommand<FocusTaskViewModel> delete,
            IRelayCommand<FocusTaskViewModel>? complete = null,
            IRelayCommand<FocusTaskViewModel>? reopen = null)
        {
            Guard.IsNotNull(task, nameof(task));
            Task = task;
            _isCompleted = task.Completed;
            DeleteCommand = delete;
            CompleteCommand = complete;
            ReopenCommand = reopen;
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                bool valueChanged = SetProperty(ref _isCompleted, value);
                if (valueChanged)
                {
                    if (value is true)
                    {
                        CompleteCommand?.Execute(this);
                    }
                    else
                    {
                        ReopenCommand?.Execute(this);
                    }
                }
            }
        }

        public FocusTask Task { get; }

        public IRelayCommand<FocusTaskViewModel> DeleteCommand { get; }

        public IRelayCommand<FocusTaskViewModel>? CompleteCommand { get; }

        public IRelayCommand<FocusTaskViewModel>? ReopenCommand { get; }

        public string Text => Task.Text;
    }
}
