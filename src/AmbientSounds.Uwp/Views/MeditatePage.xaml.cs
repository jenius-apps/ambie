using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class MeditatePage : Page
{
    public MeditatePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<MeditatePageViewModel>();
    }

    public MeditatePageViewModel ViewModel => (MeditatePageViewModel)this.DataContext;
}
