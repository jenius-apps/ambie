using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class CompactInterruptionPage : Page
{
    public CompactInterruptionPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<InterruptionPageViewModel>();
    }

    public InterruptionPageViewModel ViewModel => (InterruptionPageViewModel)this.DataContext;
}
