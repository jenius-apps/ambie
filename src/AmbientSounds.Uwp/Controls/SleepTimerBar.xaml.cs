using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SleepTimerBar : UserControl, ICanInitialize
{
    public SleepTimerBar()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SleepTimerViewModel>();
    }

    public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;

    public Task InitializeAsync()
    {
        ViewModel.Initialize();
        return Task.CompletedTask;
    }

    public void Uninitialize()
    {
        ViewModel.Uninitialize();
    }
}
