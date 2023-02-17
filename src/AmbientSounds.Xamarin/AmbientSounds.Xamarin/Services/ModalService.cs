using AmbientSounds.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services.Xamarin
{
    public class ModalService : IDialogService
    {
        public Task<string> EditTextAsync(string prepopulatedText, int? maxSize = null)
        {
            throw new NotImplementedException();
        }

        public Task MissingShareSoundsDialogAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> MissingSoundsDialogAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenHistoryDetailsAsync(FocusHistoryViewModel historyViewModel)
        {
            throw new NotImplementedException();
        }

        public Task<(double, string)> OpenInterruptionAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenPremiumAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenShareAsync(IReadOnlyList<string> soundIds)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> OpenShareResultsAsync(IList<string> soundIds)
        {
            throw new NotImplementedException();
        }

        public Task OpenThemeSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenTutorialAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenVideosMenuAsync()
        {
            throw new NotImplementedException();
        }

        public Task RecentInterruptionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> RenameAsync(string currentName)
        {
            throw new NotImplementedException();
        }
    }
}
