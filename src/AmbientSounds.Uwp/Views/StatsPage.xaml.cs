using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class StatsPage : Page
{
    public StatsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StatsPageViewModel>();
    }

    public StatsPageViewModel ViewModel { get; }
}
