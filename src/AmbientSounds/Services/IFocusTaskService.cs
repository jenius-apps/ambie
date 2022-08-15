using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusTaskService
    {
        /// <summary>
        /// Retrieves tasks.
        /// </summary>
        Task<IReadOnlyList<FocusTask>> GetTasksAsync();
    }
}