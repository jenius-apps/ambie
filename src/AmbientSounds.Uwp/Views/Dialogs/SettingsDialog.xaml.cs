using Windows.UI.Xaml.Controls;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using AmbientSounds.Services;

namespace AmbientSounds.Views.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
            ViewModel = new SettingsDialogViewModel(App.Services.GetRequiredService<IUserSettings>());
        }

        public SettingsDialogViewModel ViewModel;

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}