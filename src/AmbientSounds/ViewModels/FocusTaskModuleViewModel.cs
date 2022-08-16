using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskModuleViewModel : ObservableObject
    {
        private readonly IFocusTaskService _taskService;
        private readonly IDispatcherQueue _dispatcherQueue;
        private readonly IRelayCommand<FocusTaskViewModel> _deleteCommand;
        private readonly IRelayCommand<FocusTaskViewModel> _completeCommand;
        private readonly IRelayCommand<FocusTaskViewModel> _reopenCommand;
        private string _newTask = string.Empty;
        private bool _isCompletedListVisible;

        public FocusTaskModuleViewModel(
            IFocusTaskService focusTaskService,
            IDispatcherQueue dispatcherQueue)
        {
            Guard.IsNotNull(focusTaskService, nameof(focusTaskService));
            Guard.IsNotNull(dispatcherQueue, nameof(dispatcherQueue));

            _taskService = focusTaskService;
            _dispatcherQueue = dispatcherQueue;

            _deleteCommand = new RelayCommand<FocusTaskViewModel>(DeleteTask);
            _completeCommand = new RelayCommand<FocusTaskViewModel>(CompleteTask);
            _reopenCommand = new RelayCommand<FocusTaskViewModel>(ReopenTask);
        }

        public ObservableCollection<FocusTaskViewModel> Tasks { get; } = new();

        public ObservableCollection<FocusTaskViewModel> CompletedTasks { get; } = new();

        public int RecentCompletedCount => CompletedTasks.Count;

        public bool RecentCompletedButtonVisible => CompletedTasks.Count > 0;

        public string NewTask
        {
            get => _newTask;
            set => SetProperty(ref _newTask, value);
        }

        public bool IsCompletedListVisible
        {
            get => _isCompletedListVisible;
            set => SetProperty(ref _isCompletedListVisible, value);
        }

        public async Task InitializeAsync()
        {
            _taskService.TaskCompletionChanged += OnTaskCompletionChanged;
            CompletedTasks.CollectionChanged += OnCompletedTaskListChanged;

            if (Tasks.Count > 0)
            {
                Tasks.Clear();
            }

            var tasks = await _taskService.GetTasksAsync();
            foreach (var t in tasks)
            {
                var vm = new FocusTaskViewModel(t, _deleteCommand, _completeCommand);
                if (t.Completed)
                {
                    CompletedTasks.Add(vm);
                }
                else
                {
                    Tasks.Add(vm);
                }
            }
        }

        public void Uninitialize()
        {
            _taskService.TaskCompletionChanged -= OnTaskCompletionChanged;
            CompletedTasks.CollectionChanged -= OnCompletedTaskListChanged;
            Tasks.Clear();
            CompletedTasks.Clear();
        }

        public async Task AddTaskAsync()
        {
            var task = NewTask;
            if (string.IsNullOrWhiteSpace(task))
            {
                return;
            }

            FocusTask? newTask = await _taskService.AddTaskAsync(task);
            if (newTask is null)
            {
                return;
            }

            Tasks.Add(new FocusTaskViewModel(newTask, _deleteCommand, _completeCommand));
            NewTask = string.Empty;
        }

        private void OnCompletedTaskListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(RecentCompletedCount));
            OnPropertyChanged(nameof(RecentCompletedButtonVisible));
        }

        private void CompleteTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }

            Tasks.Remove(task);
            _ = _taskService.UpdateCompletionAsync(task.Task.Id, true).ConfigureAwait(false);
        }

        private void ReopenTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }

            CompletedTasks.Remove(task);
            _ = _taskService.UpdateCompletionAsync(task.Task.Id, false).ConfigureAwait(false);
        }

        private void DeleteTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }

            Tasks.Remove(task);

            // TODO update cache
        }

        private void OnTaskCompletionChanged(object sender, FocusTask e)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                if (e.Completed)
                {
                    CompletedTasks.Add(new FocusTaskViewModel(e, _deleteCommand, reopen: _reopenCommand));
                }
                else
                {
                    Tasks.Add(new FocusTaskViewModel(e, _deleteCommand, complete: _completeCommand));
                }
            });
        }
    }
}
