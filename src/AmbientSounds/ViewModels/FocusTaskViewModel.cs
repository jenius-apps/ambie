using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskViewModel : ObservableObject
    {
        private bool _isCompleted;
        private string _text = string.Empty;

        public FocusTaskViewModel(
            FocusTask task,
            IRelayCommand<FocusTaskViewModel>? delete = null,
            IRelayCommand<FocusTaskViewModel>? edit = null,
            IRelayCommand<FocusTaskViewModel>? complete = null,
            IRelayCommand<FocusTaskViewModel>? reopen = null)
        {
            Guard.IsNotNull(task, nameof(task));
            Task = task;
            _isCompleted = task.Completed;
            Text = task.Text;
            CompleteCommand = complete;
            ReopenCommand = reopen;

            // a fallback is used for these because they might be used with xaml binding.
            // So we ensure that if it is bound, it's not null.
            EditCommand = edit ?? new RelayCommand<FocusTaskViewModel>(vm => { });
            DeleteCommand = delete ?? new RelayCommand<FocusTaskViewModel>(vm => { }); 
        }

        public FocusTask Task { get; }

        public IRelayCommand<FocusTaskViewModel> EditCommand { get; }

        public IRelayCommand<FocusTaskViewModel> DeleteCommand { get; }

        public IRelayCommand<FocusTaskViewModel>? CompleteCommand { get; }

        public IRelayCommand<FocusTaskViewModel>? ReopenCommand { get; }

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

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}
