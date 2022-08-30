using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public class FocusTaskCache : IFocusTaskCache
    {
        private readonly SemaphoreSlim _initializeTasksLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _taskOrderLock = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<string, FocusTask>? _tasks;
        private readonly IFocusTaskRepository _focusTaskRepository;
        private string[] _taskIdPositions = Array.Empty<string>();

        public FocusTaskCache(IFocusTaskRepository focusTaskRepository)
        {
            Guard.IsNotNull(focusTaskRepository, nameof(focusTaskRepository));
            _focusTaskRepository = focusTaskRepository;
        }

        /// <inheritdoc/>
        public async Task AddTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return;
            }

            if (_tasks.TryAdd(task.Id, task))
            {
                await _taskOrderLock.WaitAsync();
                _taskIdPositions = _taskIdPositions.Concat(new[] { task.Id }).ToArray();
                _taskOrderLock.Release();

                await SaveAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return Array.Empty<FocusTask>();
            }

            // Prevent concurrency problems by holding
            // a reference of the taskIdOrder array.
            string[] taskIdOrder;
            lock (_taskIdPositions)
            {
                taskIdOrder = _taskIdPositions;
            }

            // Order the tasks from the dictionary.
            List<FocusTask> orderedTasks = new();
            foreach (var id in taskIdOrder)
            {
                if (_tasks.TryGetValue(id, out FocusTask task))
                {
                    orderedTasks.Add(task);
                }
            }

            return orderedTasks;
        }

        /// <inheritdoc/>
        public async Task<FocusTask?> GetTaskAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return null;
            }

            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return null;
            }

            return _tasks.TryGetValue(taskId, out FocusTask value) ? value : null;
        }

        /// <inheritdoc/>
        public async Task UpdateTaskAsync(FocusTask task)
        {
            if (task is null)
            {
                return;
            }

            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return;
            }

            if (_tasks.ContainsKey(task.Id))
            {
                _tasks[task.Id] = task;
                await SaveAsync();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return;
            }

            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return;
            }

            if (_tasks.TryRemove(taskId, out _))
            {
                // Remove the deleted Id from the
                // task order cache.
                await _taskOrderLock.WaitAsync();
                _taskIdPositions = _taskIdPositions.Where(x => x != taskId).ToArray();
                _taskOrderLock.Release();

                await SaveAsync();
            }
        }

        /// <inheritdoc/>
        public async Task ReorderAsync(IEnumerable<string> taskIdList)
        {
            await EnsureInitializedAsync();
            if (_tasks is null)
            {
                return;
            }

            // Ensure that the input ids
            // exist in cache.
            List<string> validIds = new();
            foreach (var id in taskIdList)
            {
                if (_tasks.ContainsKey(id))
                {
                    validIds.Add(id);
                }
            }

            // Update the task order cache.
            if (validIds.Count > 0)
            {
                await _taskOrderLock.WaitAsync();
                _taskIdPositions = validIds.ToArray();
                _taskOrderLock.Release();
            }

            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            // Retrieve and save the ordered list of tasks.
            var tasks = await GetTasksAsync();
            await _focusTaskRepository.SaveTasksAsync(tasks);
        }

        private async Task EnsureInitializedAsync()
        {
            await _initializeTasksLock.WaitAsync();

            if (_tasks is not null)
            {
                _initializeTasksLock.Release();
                return;
            }

            _tasks = new ConcurrentDictionary<string, FocusTask>();

            // Retrieve tasks from storage
            // and add to the concurrent dictionary.
            var tasks = await _focusTaskRepository.GetTasksAsync();
            foreach (var t in tasks)
            {
                _tasks.TryAdd(t.Id, t);
            }

            // Initialize the cache for the task order.
            await _taskOrderLock.WaitAsync();
            _taskIdPositions = tasks.Select(x => x.Id).ToArray();
            _taskOrderLock.Release();

            _initializeTasksLock.Release();
        }
    }
}
