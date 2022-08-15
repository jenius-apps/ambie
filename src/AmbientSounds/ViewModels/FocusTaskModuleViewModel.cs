using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskModuleViewModel : ObservableObject
    {
        private readonly IFocusTaskService _taskService;
        private string _newTask = string.Empty;

        public FocusTaskModuleViewModel(
            IFocusTaskService focusTaskService)
        {
            Guard.IsNotNull(focusTaskService, nameof(focusTaskService));
            _taskService = focusTaskService;
        }

        public ObservableCollection<FocusTaskViewModel> Tasks { get; } = new();

        public string NewTask
        {
            get => _newTask;
            set => SetProperty(ref _newTask, value);
        }

        public async Task InitializeAsync()
        {
            if (Tasks.Count > 0)
            {
                Tasks.Clear();
            }

            var tasks = await _taskService.GetTasksAsync();
            foreach (var t in tasks)
            {
                Tasks.Add(new FocusTaskViewModel(t));
            }
        }

        public void Uninitialize()
        {
            Tasks.Clear();
        }

        public void AddTask()
        {
            var task = NewTask;
            if (string.IsNullOrWhiteSpace(task))
            {
                return;
            }

            var vm = new Models.FocusTask()
            {
                Text = task.Trim()
            };

            Tasks.Add(new FocusTaskViewModel(vm));
            NewTask = string.Empty;
        }
    }
}
