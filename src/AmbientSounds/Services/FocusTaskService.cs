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

        public FocusTaskService(IFocusTaskCache focusTaskCache)
        {
            Guard.IsNotNull(focusTaskCache, nameof(focusTaskCache));
            _cache = focusTaskCache;
        }

        public async Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            IReadOnlyDictionary<string, FocusTask> tasks = await _cache.GetTasksAsync();
            return tasks.Values.ToList().AsReadOnly();
        }

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
    }
}
