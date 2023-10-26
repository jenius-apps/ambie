using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IResumeOnLaunchService
    {
        Task LoadSoundsFromPreviousSessionAsync();
        void TryResumePlayback();
    }
}