using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.Controls;

public sealed partial class FocusTaskModule : UserControl, ICanInitialize
{
    public FocusTaskModule()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<FocusTaskModuleViewModel>();
    }

    public FocusTaskModuleViewModel ViewModel => (FocusTaskModuleViewModel)this.DataContext;

    public async Task InitializeAsync()
    {
        await ViewModel.InitializeAsync();

        // Initialize chevron position if it's open
        if (ViewModel.IsCompletedListVisible)
        {
            _ = OpenChevronAnimation.StartAsync();
        }
    }

    public void Uninitialize() => ViewModel.Uninitialize();

    private async void OnTaskKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await ViewModel.AddTaskAsync();
        }
    }

    private async void OnRecentCompletedClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsCompletedListVisible)
        {
            // going to collapse the button
            await CloseChevronAnimation.StartAsync();
            ViewModel.IsCompletedListVisible = false;
        }
        else
        {
            // going to expand the button
            await OpenChevronAnimation.StartAsync();
            ViewModel.IsCompletedListVisible = true;
        }
    }

    private void OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        ViewModel.OnItemsReordered();
    }
}
