using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for renaming sounds and
    /// persisting that change.
    /// </summary>
    public class Renamer : IRenamer
    {
        private readonly ISoundService _soundDataProvider;
        private readonly IDialogService _dialogService;

        public Renamer(
            ISoundService soundDataProvider,
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
            await _soundDataProvider.UpdateSoundAsync(sound);
            return true;
        }
    }
}
