using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace AmbientSounds.Controls;

public sealed partial class DownloadMissingList : UserControl
{
    public DownloadMissingList()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<DownloadMissingViewModel>();
    }

    public DownloadMissingViewModel ViewModel => (DownloadMissingViewModel)this.DataContext;

    public async Task InitializeAsync()
    {
        await ViewModel.InitializeAsync();
    }

    public void Uninitialize() => ViewModel.Uninitialize();

    private void PremiumControl_CloseRequested(object sender, System.EventArgs e)
    {
        if (sender is FrameworkElement fe &&
            fe.Parent is FlyoutPresenter fp &&
            fp.Parent is Popup p)
        {
            p.IsOpen = false;
        }
    }
}
