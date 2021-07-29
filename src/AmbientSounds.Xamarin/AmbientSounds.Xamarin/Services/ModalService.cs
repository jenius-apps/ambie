using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services.Xamarin
{
    public class ModalService : IDialogService
    {
        public Task OpenPremiumAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenSettingsAsync()
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

        public Task<string> RenameAsync(string currentName)
        {
            throw new NotImplementedException();
        }
    }
}
