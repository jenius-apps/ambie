using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
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
        private readonly ISoundEffectsService _soundEffectsService;
        private readonly ConcurrentDictionary<string, FocusTask> _completedTasks = new();

        /// <inheritdoc/>
        public event EventHandler<FocusTask>? TaskCompletionChanged;

        public FocusTaskService(
            IFocusTaskCache focusTaskCache,
            ISoundEffectsService soundEffectsService)
        {
            Guard.IsNotNull(focusTaskCache);
            _cache = focusTaskCache;
            Guard.IsNotNull(soundEffectsService);
            _soundEffectsService = soundEffectsService;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            return _cache.GetTasksAsync();
        }

        /// <inheritdoc/>
        public IReadOnlyList<FocusTask> GetCompletedTasks()
        {
            return _completedTasks.Values.ToList();
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

            if (isCompleted)
            {
                // Scenario: task was previously open
                // and now it's being changed to completed.

                // Play the sound right away because it doesn't
                // matter if the rest of this if block fails. The
                // sound effect is inconsequential.
                _ = _soundEffectsService.Play(SoundEffect.Chime).ConfigureAwait(false);

                // Retrieve from storage.
                var storedTask = await _cache.GetTaskAsync(taskId);
                if (storedTask is null)
                {
                    return;
                }

                // Delete from storage because storage should
                // only contain open tasks.
                await _cache.DeleteAsync(taskId);

                // Mark as completed.
                storedTask.Completed = true;

                // In-memory cache so that it's removed from UI after app restart.
                _completedTasks.TryAdd(taskId, storedTask);
                TaskCompletionChanged?.Invoke(this, storedTask);
            }
            else
            {
                // Scenario: task was previously closed
                // and now it's being reopened.

                if (_completedTasks.TryRemove(taskId, out var task))
                {
                    task.Completed = false;
                    await _cache.AddTaskAsync(task);
                    TaskCompletionChanged?.Invoke(this, task);
                }
            }
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
            if (_completedTasks.ContainsKey(taskId))
            {
                _completedTasks.TryRemove(taskId, out _);
                return Task.CompletedTask;
            }
            else
            {
                return _cache.DeleteAsync(taskId);
            }
        }
    }
}
