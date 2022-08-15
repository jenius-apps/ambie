using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusTaskService
    {
        Task<FocusTask?> AddTaskAsync(string text);

        /// <summary>
        /// Retrieves tasks.
        /// </summary>
        Task<IReadOnlyList<FocusTask>> GetTasksAsync();
    }
}