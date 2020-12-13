using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class SoundSuggestionViewModel : ObservableObject
    {
        private readonly ITelemetry _telemetry;
        private readonly IUserSettings _userSettings;
        private string _suggestion = "";
        private bool _isThankYouVisible;

        public SoundSuggestionViewModel(
            ITelemetry telemetry,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _telemetry = telemetry;
            _userSettings = userSettings;

            SendSuggestionCommand = new RelayCommand<string>(SendSuggestion);
        }
        
        /// <summary>
        /// Visibility of the thank you button.
        /// </summary>
        public bool IsThankYouVisible
        {
            get => _isThankYouVisible;
            set
            {
                SetProperty(ref _isThankYouVisible, value);
                OnPropertyChanged(nameof(IsSendVisible));
            }
        }

        /// <summary>
        /// Visibility of the send button.
        /// </summary>
        public bool IsSendVisible => !IsThankYouVisible;

        /// <summary>
        /// The user's sound suggestion.
        /// </summary>
        public string Suggestion
        {
            get => _suggestion ?? "";
            set => SetProperty(ref _suggestion, value);
        }

        /// <summary>
        /// Command for sending a suggestion.
        /// </summary>
        public IRelayCommand<string> SendSuggestionCommand { get; }

        private async void SendSuggestion(string suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion) || IsThankYouVisible)
            {
                return;
            }

            IsThankYouVisible = true;
            if (suggestion == "catalogue")
            {
                _userSettings.Set(UserSettingsConstants.CataloguePreview, true);
            }
            _telemetry.SuggestSound(suggestion);
            Suggestion = "";
            await Task.Delay(1000);
            IsThankYouVisible = false;
        }
    }
}
