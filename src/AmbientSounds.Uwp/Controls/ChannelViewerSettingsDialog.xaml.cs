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
    }

    public SettingsViewModel ViewModel { get; }
}
