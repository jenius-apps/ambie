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
        private string _newTask = string.Empty;

        public FocusTaskModuleViewModel()
        {
        }

        public ObservableCollection<string> Tasks { get; } = new();

        public string NewTask
        {
            get => _newTask;
            set
            {
                SetProperty(ref _newTask, value);
            }
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(1);
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

            Tasks.Add(task.Trim());
            NewTask = string.Empty;
        }
    }
}
