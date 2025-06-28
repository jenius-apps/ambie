using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class ChannelViewerSettingsDialog : ContentDialog
{
    public ChannelViewerSettingsDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        CanChangeTimerSettings = App.Services.GetRequiredService<IFocusService>().CurrentState is Services.FocusState.None;
    }

    public SettingsViewModel ViewModel { get; }

    private bool CanChangeTimerSettings { get; }
}
