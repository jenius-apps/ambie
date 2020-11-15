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
    }
}
