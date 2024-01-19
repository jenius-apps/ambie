using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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

    private async void OnNewTaskRequested(object sender, string newTaskText)
    {
        if (string.IsNullOrEmpty(newTaskText))
        {
            return;
        }

        bool success = await ViewModel.AddTaskAsync(newTaskText);

        if (success)
        {
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.TaskAdded, new Dictionary<string, string>
            {
                { "location", "ambieMini" }
            });
        }
    }
}
