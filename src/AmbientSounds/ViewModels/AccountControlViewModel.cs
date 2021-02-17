using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class AccountControlViewModel : ObservableObject
    {
        private readonly IAccountManager _accountManager;
        private bool _signedIn;
        private bool _loading;
        private string _fullName = "";
        private string _profilePath = "";

        public AccountControlViewModel(IAccountManager accountManager)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));

            _accountManager = accountManager;

            _accountManager.SignInUpdated += OnSignInUpdated;
            SignInCommand = new RelayCommand(SignIn);
        }

        public IRelayCommand SignInCommand { get; }

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
            if (Loading)
            {
                return;
            }

            Loading = true;

            var isSignedIn = await _accountManager.IsSignedInAsync();
            await UpdatePictureAsync(isSignedIn);

            SignedIn = isSignedIn;

            Loading = false;
        }

        private async void OnSignInUpdated(object sender, bool isSignedIn)
        {
            await UpdatePictureAsync(isSignedIn);
            SignedIn = isSignedIn;
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
        }
    }
}
