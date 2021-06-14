using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for triggering a dialog
    /// or modal pop up.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Opens a settings dialog.
        /// </summary>
        Task OpenSettingsAsync();

        /// <summary>
        /// Opens the theme settings dialog.
        /// </summary>
        Task OpenThemeSettingsAsync();

        /// <summary>
        /// Opens a rename dialog.
        /// </summary>
        /// <param name="currentName">The current name use to pre-populate the field.</param>
        /// <returns>The raw value from the input text field. This can be the same as the currentName value.</returns>
        Task<string> RenameAsync(string currentName);

        /// <summary>
        /// Open dialog that shows the results of
        /// clicking on a share link.
        /// </summary>
        /// <param name="soundIds">List of Ids to preview.</param>
        /// <returns>List of sound Ids that are installed and user wants to play.
        /// Empty list if operation was cancelled.</returns>
        Task<IList<string>> OpenShareResultsAsync(IList<string> soundIds);

        /// <summary>
        /// Opens the premium dialog where users can purchase
        /// ambie plus.
        /// </summary>
        Task OpenPremiumAsync();
    }
}
