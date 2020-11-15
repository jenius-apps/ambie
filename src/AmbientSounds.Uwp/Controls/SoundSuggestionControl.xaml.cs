using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class SoundSuggestionControl : UserControl
    {
        public SoundSuggestionControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundSuggestionViewModel>();
        }

        public SoundSuggestionViewModel ViewModel => (SoundSuggestionViewModel)this.DataContext;

        private void SuggestionBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SendSuggestionCommand.Execute(ViewModel.Suggestion);
            }
        }
    }
}
