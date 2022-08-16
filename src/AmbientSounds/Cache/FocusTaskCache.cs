using AmbientSounds.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public class FocusTaskCache : IFocusTaskCache
    {
        private readonly ConcurrentDictionary<string, FocusTask> _tasks = new();

        public async Task AddTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            await Task.Delay(1);
            _tasks.TryAdd(task.Id, task);
            // update storage
        }

        public async Task<IReadOnlyDictionary<string, FocusTask>> GetTasksAsync()
        {
            // get from storage
            await Task.Delay(1);
            return _tasks;
        }

        public async Task<FocusTask?> GetTaskAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return null;
            }

            await Task.Delay(1);
            _tasks.TryGetValue(taskId, out FocusTask value);
            return value;
        }

        public async Task UpdateTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            _tasks[task.Id] = task;
            await Task.Delay(1);

            // update storage
        }
    }
}
