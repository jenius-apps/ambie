using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusTaskService
    {
        /// <summary>
        /// Raised when a tasks's Completed property is changed.
        /// </summary>
        event EventHandler<FocusTask>? TaskCompletionChanged;

        /// <summary>
        /// Adds tasks to storage.
        /// </summary>
        Task<FocusTask?> AddTaskAsync(string text);

        /// <summary>
        /// Retrieves tasks.
        /// </summary>
        Task<IReadOnlyList<FocusTask>> GetTasksAsync();

        /// <summary>
        /// Retrieves list of completed tasks.
        /// </summary>
        IReadOnlyList<FocusTask> GetCompletedTasks();

        /// <summary>
        /// Updates the the completion status of the given task.
        /// </summary>
        /// <param name="taskId">Id of task to update.</param>
        /// <param name="isCompleted">New value for IsCompleted</param>
        Task UpdateCompletionAsync(string taskId, bool isCompleted);

        /// <summary>
        /// Reorders the cached tasks based on the order of the given
        /// id list. Saves new order to storage.
        /// </summary>
        Task ReorderAsync(IEnumerable<string> taskIdList);

        /// <summary>
        /// Updates the task's text.
        /// </summary>
        /// <param name="taskId">Id of task to update.</param>
        /// <param name="newText">New value for Text.</param>
        Task UpdateTextAsync(string taskId, string newText);

        /// <summary>
        /// Deletes the given task.
        /// </summary>
        Task DeleteTaskAsync(string taskId);
    }
}