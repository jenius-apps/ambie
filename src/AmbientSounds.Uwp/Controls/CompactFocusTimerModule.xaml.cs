using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class CompactFocusTimerModule : UserControl, ICanInitialize
{
    public CompactFocusTimerModule()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<FocusTimerModuleViewModel>();
    }

    public FocusTimerModuleViewModel ViewModel => (FocusTimerModuleViewModel)this.DataContext;

    public async Task InitializeAsync()
    {
        await ViewModel.InitializeAsync();
    }

    public void Uninitialize()
    {
        ViewModel.Uninitialize();
    }
}
