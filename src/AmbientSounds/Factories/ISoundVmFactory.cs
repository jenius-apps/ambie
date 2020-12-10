using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories
{
    /// <summary>
    /// Interface for creating sound viewmodels.
    /// </summary>
    public interface ISoundVmFactory
    {
        /// <summary>
        /// Creates a new online sound viewmodel.
        /// </summary>
        /// <param name="s">The sound to associate with the viewmodel.</param>
        /// <returns>An online sound viewmodel.</returns>
        OnlineSoundViewModel GetOnlineSoundVm(Sound s);
    }
}