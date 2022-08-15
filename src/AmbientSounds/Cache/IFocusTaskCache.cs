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
    }
}
