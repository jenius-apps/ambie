using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;
public sealed partial class SleepTimerBar : UserControl
{
    public SleepTimerBar()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SleepTimerViewModel>();
    }

    public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;

    public void Initialize() => ViewModel.Initialize();

    public void Uninitialize() => ViewModel.Uninitialize();
}
