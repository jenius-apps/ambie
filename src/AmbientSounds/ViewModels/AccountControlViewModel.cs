using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class AccountControlViewModel : ObservableObject
    {
        private readonly IAccountManager _accountManager;
        private bool _signedIn;
        private bool _loading;
        private string _fullName = "";

        public AccountControlViewModel(IAccountManager accountManager)
        {
            Guard.IsNotNull(accountManager, nameof(accountManager));

            _accountManager = accountManager;
        }

        /// <summary>
        /// Full display name of the user.
        /// </summary>
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

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

            SignedIn = await _accountManager.IsSignedInAsync();
            await Task.Delay(3000);

            if (SignedIn)
            {
                // var user = await _accountManager.GetUserAsync();
                // FullName = user.DisplayName;
                FullName = "Daniel Paulino";
            }

            Loading = false;
        }
    }
}
