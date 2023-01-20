using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public partial class AccountControlViewModel : ObservableObject
    {
        private readonly IAccountManager _accountManager;
        private readonly ITelemetry _telemetry;
        private readonly ISyncEngine _syncEngine;
        private readonly ISystemInfoProvider _systemInfoProvider;

        /// <summary>
        /// Determines if the user is signed in or not.
        /// </summary>
        [ObservableProperty]
        private bool _signedIn;

        /// <summary>
        /// Determines if the account control is loading.
        /// </summary>
        [ObservableProperty]
        private bool _loading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProfilePath))]
        [NotifyPropertyChangedFor(nameof(IsProfilePathValid))]
        [NotifyPropertyChangedFor(nameof(FirstName))]
        [NotifyPropertyChangedFor(nameof(Email))]
        private Person? _person;

        public AccountControlViewModel(
            IAccountManager accountManager,
            ITelemetry telemetry,
            ISyncEngine syncEngine,
            ISystemInfoProvider systemInfoProvider)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(syncEngine, nameof(syncEngine));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));

            _systemInfoProvider = systemInfoProvider;
            _accountManager = accountManager;
            _telemetry = telemetry;
            _syncEngine = syncEngine;
        }

        /// <summary>
        /// Determines if the app is in Ten Foot PC or Xbox mode.
        /// Used to determine if upload button should be displayed or not.
        /// </summary>
        public bool IsNotTenFoot => !_systemInfoProvider.IsTenFoot();

        /// <summary>
        /// First name of the user.
        /// </summary>
        public string FirstName => Person?.Firstname ?? "";

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

        public async Task LoadAsync()
        {
            _accountManager.SignInUpdated += OnSignInUpdated;
            _syncEngine.SyncStarted += OnSyncStarted;
            _syncEngine.SyncCompleted += OnSyncCompleted;


            if (Loading || SignedIn)
            {
                return;
            }

            Loading = true;
            bool isSignedIn;

            try
            {
                isSignedIn = await _accountManager.IsSignedInAsync();
                await UpdatePictureAsync(isSignedIn);
            }
            catch
            {
                isSignedIn = false;
            }

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

        [RelayCommand]
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

        [RelayCommand]
        private async Task SyncAsync()
        {
            await _syncEngine.SyncDown();
            _telemetry.TrackEvent(TelemetryConstants.SyncManual);
        }

        [RelayCommand]
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
