using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

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
