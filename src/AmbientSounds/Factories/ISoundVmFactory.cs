using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;

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

        /// <summary>
        /// Creates new sound viewmodel.
        /// </summary>
        /// <param name="s">The sound to associate with the viewmodel.</param>
        /// <returns>A sound viewmodel.</returns>
        SoundViewModel GetSoundVm(Sound s);

        /// <summary>
        /// Creates new active track viewmodel.
        /// </summary>
        /// <param name="s">The related sound.</param>
        /// <param name="removeCommand">A command that removes the active track from the active track list.</param>
        /// <returns>An active track viewmodel.</returns>
        ActiveTrackViewModel GetActiveTrackVm(Sound s, IRelayCommand<Sound> removeCommand);

        VersionedAssetViewModel GetVersionAssetVm(IVersionedAsset versionedAsset, UpdateReason updateReason);
    }
}