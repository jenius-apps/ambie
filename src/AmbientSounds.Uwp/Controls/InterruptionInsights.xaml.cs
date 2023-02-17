using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class InterruptionInsights : UserControl
{
    public InterruptionInsights()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<InterruptionInsightsViewModel>();
    }

    public InterruptionInsightsViewModel ViewModel => (InterruptionInsightsViewModel)this.DataContext;

    public async Task InitializeAsync()
    {
        await ViewModel.InitializeAsync();
    }

    public void Uninitialize()
    {
        ViewModel.Uninitialize();
    }
}
