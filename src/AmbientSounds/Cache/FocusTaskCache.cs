using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
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
        private readonly IFocusTaskRepository _focusTaskRepository;

        public FocusTaskCache(IFocusTaskRepository focusTaskRepository)
        {
            Guard.IsNotNull(focusTaskRepository, nameof(focusTaskRepository));
            _focusTaskRepository = focusTaskRepository;
        }

        public async Task AddTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            _tasks.TryAdd(task.Id, task);
            await _focusTaskRepository.SaveTasksAsync(_tasks.Values);
        }

        public async Task<IReadOnlyDictionary<string, FocusTask>> GetTasksAsync()
        {
            await EnsureInitializedAsync();
            return _tasks;
        }

        public async Task<FocusTask?> GetTaskAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return null;
            }

            await EnsureInitializedAsync();
            return _tasks.TryGetValue(taskId, out FocusTask value) ? value : null;
        }

        public async Task UpdateTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            await EnsureInitializedAsync();

            if (_tasks.ContainsKey(task.Id))
            {
                _tasks[task.Id] = task;
                await _focusTaskRepository.SaveTasksAsync(_tasks.Values);
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (_tasks.Count > 0)
            {
                return;
            }

            var tasks = await _focusTaskRepository.GetTasksAsync();
            foreach (var t in tasks)
            {
                _tasks.TryAdd(t.Id, t);
            }
        }
    }
}
