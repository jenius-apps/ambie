using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SettingsViewModel>();
        }

        public SettingsViewModel ViewModel => (SettingsViewModel)this.DataContext;

        private string Version => SystemInformation.Instance.ApplicationVersion.ToFormattedString();

        private void OnImageClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is string imagePath)
            {
                ViewModel.SelectImageCommand.Execute(imagePath);
            }
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems is [ComboBoxItem c, ..] && c.Tag is string s)
            {
                ViewModel.UpdateTheme(s);
            }
        }
    }
}