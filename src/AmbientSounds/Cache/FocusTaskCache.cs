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
        }

        public async Task<IReadOnlyDictionary<string, FocusTask>> GetTasksAsync()
        {
            await Task.Delay(1);
            return _tasks;
        }
    }
}
