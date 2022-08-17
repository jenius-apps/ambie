using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusTaskService
    {
        event EventHandler<FocusTask>? TaskCompletionChanged;

        Task<FocusTask?> AddTaskAsync(string text);

        /// <summary>
        /// Retrieves tasks.
        /// </summary>
        Task<IReadOnlyList<FocusTask>> GetTasksAsync();

        /// <summary>
        /// Updates the the completion status of the given task.
        /// </summary>
        /// <param name="taskId">Id of task to update.</param>
        /// <param name="isCompleted">New value for IsCompleted</param>
        Task UpdateCompletionAsync(string taskId, bool isCompleted);

        /// <summary>
        /// Deletes the given task.
        /// </summary>
        Task DeleteTaskAsync(string taskId);
    }
}