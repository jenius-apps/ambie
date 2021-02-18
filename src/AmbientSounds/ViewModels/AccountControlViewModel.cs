using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class AccountControlViewModel : ObservableObject
    {
        private readonly IAccountManager _accountManager;
        private readonly ITelemetry _telemetry;
        private bool _signedIn;
        private bool _loading;
        private string _fullName = "";
        private string _profilePath = "";

        public AccountControlViewModel(
            IAccountManager accountManager,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _accountManager = accountManager;
            _telemetry = telemetry;

            _accountManager.SignInUpdated += OnSignInUpdated;
            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        }

        public IRelayCommand SignInCommand { get; }

        public IAsyncRelayCommand SignOutCommand { get; }

        /// <summary>
        /// Full display name of the user.
        /// </summary>
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        /// <summary>
        /// Path to user's profile image.
        /// </summary>
        public string ProfilePath
        {
            get => string.IsNullOrWhiteSpace(_profilePath) ? "http://localhost:8000" : _profilePath;
            set 
            {
                SetProperty(ref _profilePath, value);
                OnPropertyChanged(nameof(IsProfilePathValid));
            } 
        }

        /// <summary>
        /// Determines if the profile picture path is a valid string.
        /// </summary>
        public bool IsProfilePathValid => !string.IsNullOrWhiteSpace(_profilePath);

        /// <summary>
        /// Determines if the user is signed in or not.
        /// </summary>
        public bool SignedIn
        {
            get => _signedIn;
            set => SetProperty(ref _signedIn, value);
        }

        /// <summary>
        /// Determines if the account control is loading.
        /// </summary>
        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public async void Load()
        {
            if (Loading || _signedIn)
            {
                return;
            }

            Loading = true;

            var isSignedIn = await _accountManager.IsSignedInAsync();
            await UpdatePictureAsync(isSignedIn);

            SignedIn = isSignedIn;

            if (isSignedIn)
            {
                _telemetry.TrackEvent(TelemetryConstants.SilentSuccessful);
            }

            Loading = false;
        }

        private async Task SignOutAsync()
        {
            await _accountManager.SignOutAsync();
            _telemetry.TrackEvent(TelemetryConstants.SignOutClicked);
        }

        private async void OnSignInUpdated(object sender, bool isSignedIn)
        {
            await UpdatePictureAsync(isSignedIn);
            SignedIn = isSignedIn;
            _telemetry.TrackEvent(TelemetryConstants.SignInCompleted, new Dictionary<string, string>
            {
                { "isSignedIn", isSignedIn.ToString() }
            });
        }

        private async Task UpdatePictureAsync(bool isSignedIn)
        {
            ProfilePath = "";

            if (isSignedIn)
            {
                var path = await _accountManager.GetPictureAsync();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    ProfilePath = path;
                }
            }
        }

        private void SignIn()
        {
            _accountManager.RequestSignIn();
            _telemetry.TrackEvent(TelemetryConstants.SignInTriggered);
        }
    }
}
