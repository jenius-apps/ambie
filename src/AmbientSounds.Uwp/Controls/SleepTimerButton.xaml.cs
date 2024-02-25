using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class SleepTimerButton : UserControl
{
    public SleepTimerButton()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SleepTimerViewModel>();
    }

    public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;

    public void Initialize() => ViewModel.Initialize();

    public void Uninitialize() => ViewModel.Uninitialize();
}
