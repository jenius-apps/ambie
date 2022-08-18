﻿using AmbientSounds.Constants;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskModuleViewModel : ObservableObject
    {
        private const int MaxTaskText = 280;
        private readonly IFocusTaskService _taskService;
        private readonly IDispatcherQueue _dispatcherQueue;
        private readonly IDialogService _dialogService;
        private readonly ITelemetry _telemetry;
        private readonly IFocusHistoryService _focusHistoryService;
        private readonly IRelayCommand<FocusTaskViewModel> _deleteCommand;
        private readonly IRelayCommand<FocusTaskViewModel> _completeCommand;
        private readonly IRelayCommand<FocusTaskViewModel> _reopenCommand;
        private readonly IRelayCommand<FocusTaskViewModel> _editCommand;
        private string _newTask = string.Empty;
        private bool _isCompletedListVisible;

        public FocusTaskModuleViewModel(
            IFocusTaskService focusTaskService,
            IDispatcherQueue dispatcherQueue,
            IDialogService dialogService,
            IFocusHistoryService focusHistoryService,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(focusTaskService, nameof(focusTaskService));
            Guard.IsNotNull(dispatcherQueue, nameof(dispatcherQueue));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(focusHistoryService, nameof(focusHistoryService));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _taskService = focusTaskService;
            _dispatcherQueue = dispatcherQueue;
            _dialogService = dialogService;
            _focusHistoryService = focusHistoryService;
            _telemetry = telemetry;

            _deleteCommand = new RelayCommand<FocusTaskViewModel>(DeleteTask);
            _completeCommand = new RelayCommand<FocusTaskViewModel>(CompleteTask);
            _reopenCommand = new RelayCommand<FocusTaskViewModel>(ReopenTask);
            _editCommand = new RelayCommand<FocusTaskViewModel>(EditTask);
        }

        public int MaxTextSize => MaxTaskText;

        public ObservableCollection<FocusTaskViewModel> Tasks { get; } = new();

        public ObservableCollection<FocusTaskViewModel> CompletedTasks { get; } = new();

        public int RecentCompletedCount => CompletedTasks.Count;

        public bool RecentCompletedButtonVisible => CompletedTasks.Count > 0;

        public bool OpenTaskListVisible => Tasks.Count > 0;

        public bool CanAddMoreTasks => Tasks.Count < 5;

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
            if (Tasks.Count > 0)
            {
                Tasks.Clear();
            }

            _taskService.TaskCompletionChanged += OnTaskCompletionChanged;
            CompletedTasks.CollectionChanged += OnCompletedTaskListChanged;
            Tasks.CollectionChanged += OnOpenTasksChanged;

            var tasks = await _taskService.GetTasksAsync();
            foreach (var t in tasks)
            {
                if (!t.Completed)
                {
                    Tasks.Add(CreateTaskVm(t, false));
                }
            }

            foreach (var c in _taskService.GetCompletedTasks())
            {
                if (c.Completed)
                {
                    CompletedTasks.Add(CreateTaskVm(c, true));
                }
            }

            _telemetry.TrackEvent(TelemetryConstants.TasksLoaded, new Dictionary<string, string>
            {
                { "openCount", Tasks.Count.ToString() },
                { "completedCount", CompletedTasks.Count.ToString() },
            });
        }

        public void Uninitialize()
        {
            _taskService.TaskCompletionChanged -= OnTaskCompletionChanged;
            CompletedTasks.CollectionChanged -= OnCompletedTaskListChanged;
            Tasks.CollectionChanged -= OnOpenTasksChanged;
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

            Tasks.Add(CreateTaskVm(newTask, false));
            NewTask = string.Empty;
            _telemetry.TrackEvent(TelemetryConstants.TaskAdded);
        }

        public void OnItemsReordered()
        {
            if (Tasks.Count == 0)
            {
                return;
            }

            _telemetry.TrackEvent(TelemetryConstants.TaskReordered);
            _ = _taskService.ReorderAsync(Tasks.Select(x => x.Task.Id)).ConfigureAwait(false);
        }

        private void OnCompletedTaskListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(RecentCompletedCount));
            OnPropertyChanged(nameof(RecentCompletedButtonVisible));
        }

        private void OnOpenTasksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(OpenTaskListVisible));
            OnPropertyChanged(nameof(CanAddMoreTasks));
        }

        private void CompleteTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }

            Tasks.Remove(task);
            _focusHistoryService.LogTaskCompleted(task.Task.Id);
            _ = _taskService.UpdateCompletionAsync(task.Task.Id, true).ConfigureAwait(false);
            _telemetry.TrackEvent(TelemetryConstants.TaskCompleted);
        }

        private void ReopenTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }

            CompletedTasks.Remove(task);
            _focusHistoryService.RevertTaskCompleted(task.Task.Id);
            _ = _taskService.UpdateCompletionAsync(task.Task.Id, false).ConfigureAwait(false);
            _telemetry.TrackEvent(TelemetryConstants.TaskReopened);
        }

        private void DeleteTask(FocusTaskViewModel? task)
        {
            if (task is null)
            {
                return;
            }
            
            if (task.IsCompleted)
            {
                CompletedTasks.Remove(task);
            }
            else
            {
                Tasks.Remove(task);
            }

            _ = _taskService.DeleteTaskAsync(task.Task.Id).ConfigureAwait(false);
            _telemetry.TrackEvent(TelemetryConstants.TaskDeleted, new Dictionary<string, string>
            {
                { "completed", task.IsCompleted.ToString() }
            });
        }

        private async void EditTask(FocusTaskViewModel? task)
        {
            if (task is null || task.IsCompleted)
            {
                // we do not allow editing of completed tasks.
                return;
            }

            string? newText = await _dialogService.EditTextAsync(task.Text);
            if (!string.IsNullOrEmpty(newText))
            {
                // Update the UI
                task.Text = newText!;

                // Update the cache
                _ = _taskService.UpdateTextAsync(task.Task.Id, newText!).ConfigureAwait(false);
                _telemetry.TrackEvent(TelemetryConstants.TaskEdited);
            }
        }

        private void OnTaskCompletionChanged(object sender, FocusTask e)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                if (e.Completed)
                {
                    CompletedTasks.Add(CreateTaskVm(e, true));
                }
                else
                {
                    Tasks.Add(CreateTaskVm(e, false));
                }
            });
        }

        private FocusTaskViewModel CreateTaskVm(FocusTask task, bool completed)
        {
            return completed
                ? new FocusTaskViewModel(
                    task,
                    delete: _deleteCommand,
                    reopen: _reopenCommand)
                : new FocusTaskViewModel(
                    task,
                    delete: _deleteCommand,
                    edit: _editCommand,
                    complete: _completeCommand);
        }
    }
}
