using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class CompactFocusTimerModule : UserControl
{
    public CompactFocusTimerModule()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<FocusTimerModuleViewModel>();
    }

    public FocusTimerModuleViewModel ViewModel => (FocusTimerModuleViewModel)this.DataContext;

    public async Task InitializeAsync(bool allowSoundPausing)
    {
        await ViewModel.InitializeAsync(allowSoundPausing);
    }

    public void Uninitialize()
    {
        ViewModel.Uninitialize();
    }

    private async void OnTaskChangeRequested(object sender, Events.TaskTickerChangeRequested e)
    {
        if (string.IsNullOrEmpty(e.NewText))
        {
            return;
        }

        if (e.ChangeType is Events.TaskTickerChangeType.Add)
        {

            bool success = await ViewModel.AddTaskAsync(e.NewText);

            if (success)
            {
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.TaskAdded, new Dictionary<string, string>
                {
                    { "location", "ambieMini" }
                });
            }
        }
        else if (e.ChangeType is Events.TaskTickerChangeType.Edit && e.Index is int index)
        {
            bool success = await ViewModel.EditTaskAsync(e.NewText, index);
            if (success)
            {
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.TaskEdited, new Dictionary<string, string>
                {
                    { "location", "ambieMini" }
                });
            }
        }
    }
}
