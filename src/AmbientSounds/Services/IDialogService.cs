using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

/// <summary>
/// Interface for triggering a dialog
/// or modal pop up.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Opens the sound dialog.
    /// </summary>
    /// <param name="vm">The online sound to use.</param>
    /// <returns>True if the primary action was clicked.</returns>
    Task<bool> OpenSoundDialogAsync(OnlineSoundViewModel vm);

    /// <summary>
    /// Opens a rename dialog.
    /// </summary>
    /// <param name="currentName">The current name use to pre-populate the field.</param>
    /// <returns>The raw value from the input text field. This can be the same as the currentName value.</returns>
    Task<string> RenameAsync(string currentName);

    /// <summary>
    /// Opens the premium dialog where users can purchase
    /// ambie plus.
    /// </summary>
    Task OpenPremiumAsync();

    /// <summary>
    /// Opens dialog regarding missing sounds.
    /// Returns true if user chooses to
    /// download missing sounds.
    /// </summary>
    Task<bool> MissingSoundsDialogAsync();

    /// <summary>
    /// Opens videos menu dialog.
    /// </summary>
    Task OpenVideosMenuAsync();

    /// <summary>
    /// Opens interruption dialog.
    /// </summary>
    Task<(double, string)> OpenInterruptionAsync();

    /// <summary>
    /// Opens dialog for the given history details.
    /// </summary>
    Task OpenHistoryDetailsAsync(FocusHistoryViewModel historyViewModel);

    /// <summary>
    /// Opens a dialog that will edit the prepopulated string.
    /// </summary>
    /// <param name="prepopulatedText">Text to prepopulate the UI.</param>
    /// <param name="maxSize">Optional max length for the string input.</param>
    /// <returns>
    /// Returns a real string if the operation was confirmed, the text was valid, and if the text was changed.
    /// </returns>
    Task<string?> EditTextAsync(string prepopulatedText, int? maxSize = null);
    
    /// <summary>
    /// Opens the tutorial dialog.
    /// </summary>
    Task OpenTutorialAsync();

    /// <summary>
    /// Opens the share dialog.
    /// </summary>
    Task OpenShareAsync(IReadOnlyList<string> soundIds);

    /// <summary>
    /// Opens the missing shared sounds dialog.
    /// </summary>
    Task MissingShareSoundsDialogAsync();

    /// <summary>
    /// Opens dialog for recent interruptions.
    /// </summary>
    Task RecentInterruptionsAsync();
}
