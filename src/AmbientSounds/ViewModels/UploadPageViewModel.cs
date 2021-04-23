using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// ViewModel representing the upload page.
    /// </summary>
    public class UploadPageViewModel
    {
        private readonly INavigator _navigator;

        public UploadPageViewModel(INavigator navigator)
        {
            Guard.IsNotNull(navigator, nameof(navigator));

            _navigator = navigator;
        }

        /// <summary>
        /// Navigates the frame backwards.
        /// </summary>
        public void GoBack() => _navigator.GoBack();
    }
}
