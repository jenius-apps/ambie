using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface ISoundEffectsService
    {
        Task Play(SoundEffect effect);
    }

    public enum SoundEffect
    {
        Chime,
        Bell
    }
}