using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IFocusTaskRepository
    {
        /// <summary>
        /// Saves the give list to storage.
        /// </summary>
        Task SaveTasksAsync(IEnumerable<FocusTask> tasks);

        /// <summary>
        /// Retrieves list of tasks from storage.
        /// </summary>
        Task<IReadOnlyList<FocusTask>> GetTasksAsync();
    }
}
