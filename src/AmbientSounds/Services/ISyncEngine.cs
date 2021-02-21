using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface ISyncEngine
    {
        event EventHandler? SyncCompleted;
        event EventHandler? SyncStarted;

        Task SyncUp();
    }
}