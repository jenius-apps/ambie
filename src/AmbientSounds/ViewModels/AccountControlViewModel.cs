using AmbientSounds.Constants;
using AmbientSounds.Models;
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
        private readonly ISyncEngine _syncEngine;
        private bool _signedIn;
        private bool _loading;
        private Person? _person;

        public AccountControlViewModel(
            IAccountManager accountManager,
            ITelemetry telemetry,
            ISyncEngine syncEngine)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(syncEngine, nameof(syncEngine));

            _accountManager = accountManager;
            _telemetry = telemetry;
            _syncEngine = syncEngine;

            _accountManager.SignInUpdated += OnSignInUpdated;
            _syncEngine.SyncStarted += OnSyncStarted;
            _syncEngine.SyncCompleted += OnSyncCompleted;

            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        }

        public IRelayCommand SignInCommand { get; }

        public IAsyncRelayCommand SignOutCommand { get; }

        public Person? Person
        {
            get => _person;
            set 
            {
                SetProperty(ref _person, value);
                OnPropertyChanged(nameof(ProfilePath));
                OnPropertyChanged(nameof(IsProfilePathValid));
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(Email));
            }
        }

        /// <summary>
        /// First name of the user.
        /// </summary>
        public string FirstName => _person?.Firstname ?? "";

        /// <summary>
        /// Email of the user.
        /// </summary>
        public string Email => _person?.Email ?? "";

        /// <summary>
        /// Path to user's profile image.
        /// </summary>
        public string ProfilePath => Person == null || string.IsNullOrWhiteSpace(Person.PicturePath) 
            ? "http://localhost:8000" 
            : Person.PicturePath;
       
        /// <summary>
        /// Determines if the profile picture path is a valid string.
        /// </summary>
        public bool IsProfilePathValid => !string.IsNullOrWhiteSpace(Person?.PicturePath);

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

        private void OnSyncCompleted(object sender, System.EventArgs e)
        {
            Loading = false;
        }

        private void OnSyncStarted(object sender, System.EventArgs e)
        {
            Loading = true;
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
            Person = null;

            if (isSignedIn)
            {
                Person = await _accountManager.GetPersonDataAsync();
            }
        }

        private void SignIn()
        {
            _accountManager.RequestSignIn();
            _telemetry.TrackEvent(TelemetryConstants.SignInTriggered);
        }
    }
}
