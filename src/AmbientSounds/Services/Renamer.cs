using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for renaming sounds and
    /// persisting that change.
    /// </summary>
    public class Renamer : IRenamer
    {
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IDialogService _dialogService;

        public Renamer(
            ISoundDataProvider soundDataProvider,
            IDialogService dialogService)
        {
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(dialogService, nameof(dialogService));

            _dialogService = dialogService;
            _soundDataProvider = soundDataProvider;
        }

        /// <inheritdoc/>
        public async Task<bool> RenameAsync(Sound sound)
        {
            if (sound is null)
            {
                return false;
            }

            string newName = await _dialogService.RenameAsync(sound.Name);
            if (string.IsNullOrWhiteSpace(newName) || newName == sound.Name)
            {
                return false;
            }

            sound.Name = newName.Trim();
            await _soundDataProvider.UpdateLocalSoundAsync(new Sound[] { sound });
            return true;
        }
    }
}
