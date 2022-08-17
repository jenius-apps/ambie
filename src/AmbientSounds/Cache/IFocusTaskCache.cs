using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IFocusTaskCache
    {
        Task AddTaskAsync(FocusTask task);

        Task<IReadOnlyDictionary<string, FocusTask>> GetTasksAsync();

        Task<FocusTask?> GetTaskAsync(string taskId);

        Task UpdateTaskAsync(FocusTask task);

        Task DeleteAsync(string taskId);
    }
}
