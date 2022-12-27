using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public interface ISoundService
{
    /// <summary>
    /// Local sound added.
    /// </summary>
    event EventHandler<Sound> LocalSoundAdded;

    /// <summary>
    /// Local sound deleted.
    /// </summary>
    event EventHandler<string> LocalSoundDeleted;

    /// <summary>
    /// Gets an installed sound from its id, if present.
    /// </summary>
    /// <param name="soundId">The id of the installed sound to retrieve.</param>
    /// <returns>The installed sound with the provided id, if it exists.</returns>
    Task<Sound?> GetLocalSoundAsync(string? soundId);

    /// <summary>
    /// Gets the list of sounds that are installed.
    /// </summary>
    /// <returns>List of installed sound objects.</returns>
    Task<IReadOnlyList<Sound>> GetLocalSoundsAsync(IReadOnlyList<string>? soundIds = null);

    /// <summary>
    /// Takes the packaged sounds and installs them
    /// if the installed sound list is empty.
    /// </summary>
    /// <remarks>
    /// Often used when the app is first used
    /// after being installed by the user.
    /// </remarks>
    Task PrepopulateSoundsIfEmpty();

    /// <summary>
    /// Returns true if the given sound ID is installed;
    /// </summary>
    Task<bool> IsSoundInstalledAsync(string id);

    /// <summary>
    /// Adds sound info to local list.
    /// </summary>
    /// <param name="s">The sound info to save.</param>
    Task AddLocalSoundAsync(Sound? s);

    /// <summary>
    /// Deletes sound info from local list.
    /// </summary>
    /// <param name="id">The sound info to delete.</param>
    Task DeleteLocalSoundAsync(string id);

    /// <summary>
    /// Retrieves a random sound.
    /// </summary>
    Task<Sound?> GetRandomSoundAsync();

    /// <summary>
    /// Updates the given sounds in cache and in storage.
    /// </summary>
    /// <param name="sounds">The sounds to update.</param>
    Task UpdateSoundAsync(Sound sound);

    /// <summary>
    /// Updates the position of the given sound
    /// and the positions of the surrounding sounds.
    /// </summary>
    /// <param name="soundId">The ID of the sound that was moved.</param>
    /// <param name="oldIndex">The sound's old position.</param>
    /// <param name="newIndex">The sound's new position.</param>
    Task UpdatePositionsAsync(string soundId, int oldIndex, int newIndex);
}