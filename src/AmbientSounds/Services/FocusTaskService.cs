using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public class FocusTaskService : IFocusTaskService
    {
        public async Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            await Task.Delay(1);
            return Array.Empty<FocusTask>();
        }
    }
}
