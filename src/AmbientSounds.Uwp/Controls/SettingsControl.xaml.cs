using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SettingsViewModel>();

            ViewModel.AvailableLanugages.Clear();//Probably unncessary.
            ViewModel.AvailableLanugages.AddRange(ApplicationLanguages.Languages);//Looks like only the languages supported by both the OS and application are listed here.
        }

        public SettingsViewModel ViewModel => (SettingsViewModel)this.DataContext;
    }
}