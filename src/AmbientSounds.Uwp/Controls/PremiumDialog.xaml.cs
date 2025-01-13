using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class PremiumDialog : ContentDialog
{
    public PremiumDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PremiumControlViewModel>();
    }
    
    public PremiumControlViewModel ViewModel { get; }

    private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        try
        {
            await ViewModel.InitializeAsync();
        }
        catch { }
    }

    private void CloseClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        this.Hide();
    }
}
