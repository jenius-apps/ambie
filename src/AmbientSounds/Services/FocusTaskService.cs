using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services
{
    public class FocusTaskService : IFocusTaskService
    {
        private readonly IFocusTaskCache _cache;

        /// <inheritdoc/>
        public event EventHandler<FocusTask>? TaskCompletionChanged;

        public FocusTaskService(IFocusTaskCache focusTaskCache)
        {
            Guard.IsNotNull(focusTaskCache, nameof(focusTaskCache));
            _cache = focusTaskCache;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            return _cache.GetTasksAsync();
        }

        /// <inheritdoc/>
        public Task<FocusTask?> AddTaskAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult<FocusTask?>(null);
            }

            var newTask = new FocusTask
            {
                Id = Guid.NewGuid().ToString(),
                Text = text.Trim(),
                CreatedDate = DateTime.Now
            };

            _ = _cache.AddTaskAsync(newTask).ConfigureAwait(false);

            return Task.FromResult<FocusTask?>(newTask);
        }

        /// <inheritdoc/>
        public async Task UpdateCompletionAsync(string taskId, bool isCompleted)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return;
            }

            var task = await _cache.GetTaskAsync(taskId);
            if (task is null)
            {
                return;
            }

            task.Completed = isCompleted;
            await _cache.UpdateTaskAsync(task);
            TaskCompletionChanged?.Invoke(this, task);
        }

        /// <inheritdoc/>
        public Task ReorderAsync(IEnumerable<string> taskIdList)
        {
            return _cache.ReorderAsync(taskIdList);
        }

        /// <inheritdoc/>
        public async Task UpdateTextAsync(string taskId, string newText)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return;
            }

            var task = await _cache.GetTaskAsync(taskId);
            if (task is null || task.Completed)
            {
                // We do not allow editing of completed tasks.
                return;
            }

            task.Text = newText;
            await _cache.UpdateTaskAsync(task);
        }

        /// <inheritdoc/>
        public Task DeleteTaskAsync(string taskId)
        {
            return _cache.DeleteAsync(taskId);
        }
    }
}
