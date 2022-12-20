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
    /// Gets the list of sounds that are installed.
    /// </summary>
    /// <returns>List of installed sound objects.</returns>
    Task<IReadOnlyList<Sound>> GetLocalSoundsAsync();

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
}