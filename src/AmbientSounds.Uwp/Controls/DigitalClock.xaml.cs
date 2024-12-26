using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class DigitalClock : UserControl
{
    public DigitalClock()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<DigitalClockViewModel>();
    }

    public DigitalClockViewModel ViewModel { get; }

    public void Initialize() => ViewModel.Initialize();

    public void Uninitialize() => ViewModel.Uninitialize();
}
