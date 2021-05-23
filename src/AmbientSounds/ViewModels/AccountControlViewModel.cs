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
        private readonly INavigator _navigator;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private bool _signedIn;
        private bool _loading;
        private Person? _person;

        public AccountControlViewModel(
            IAccountManager accountManager,
            ITelemetry telemetry,
            ISyncEngine syncEngine,
            ISystemInfoProvider systemInfoProvider,
            INavigator navigator)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(syncEngine, nameof(syncEngine));
            Guard.IsNotNull(navigator, nameof(navigator));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));

            _systemInfoProvider = systemInfoProvider;
            _accountManager = accountManager;
            _telemetry = telemetry;
            _syncEngine = syncEngine;
            _navigator = navigator;

            SignInCommand = new AsyncRelayCommand(SignIn);
            OpenUploadPageCommand = new RelayCommand(UploadClicked);
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
            SyncCommand = new AsyncRelayCommand(SyncAsync);
        }

        public IAsyncRelayCommand SignInCommand { get; }

        public IRelayCommand OpenUploadPageCommand { get; }

        public IAsyncRelayCommand SignOutCommand { get; }

        public IAsyncRelayCommand SyncCommand { get; }

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
        /// Determines if the app is in Ten Foot PC or Xbox mode.
        /// Used to determine if upload button should be displayed or not.
        /// </summary>
        public bool IsNotTenFoot => !_systemInfoProvider.IsTenFoot();

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
        public string ProfilePath => Person is null || string.IsNullOrWhiteSpace(Person.PicturePath) 
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

        public async Task LoadAsync()
        {
            _accountManager.SignInUpdated += OnSignInUpdated;
            _syncEngine.SyncStarted += OnSyncStarted;
            _syncEngine.SyncCompleted += OnSyncCompleted;


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

        private void UploadClicked()
        {
            _navigator.ToUploadPage();
        }

        private async Task SyncAsync()
        {
            await _syncEngine.SyncDown();
            _telemetry.TrackEvent(TelemetryConstants.SyncManual);
        }

        private async Task SignIn()
        {
            await _accountManager.RequestSignIn();
            _telemetry.TrackEvent(TelemetryConstants.SignInTriggered);
        }

        public void Dispose()
        {
            _accountManager.SignInUpdated -= OnSignInUpdated;
            _syncEngine.SyncStarted -= OnSyncStarted;
            _syncEngine.SyncCompleted -= OnSyncCompleted;
        }
    }
}
