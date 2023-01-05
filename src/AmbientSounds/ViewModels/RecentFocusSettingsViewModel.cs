using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels;

public class RecentFocusSettingsViewModel : ObservableObject
{
    public RecentFocusSettingsViewModel(
        RecentFocusSettings settings,
        IAsyncRelayCommand<RecentFocusSettingsViewModel?> deleteCommand)
    {
        Guard.IsNotNull(settings);
        Guard.IsNotNull(deleteCommand);
        Settings = settings;
        DeleteCommand = deleteCommand;
    }

    public RecentFocusSettings Settings { get; }

    public IAsyncRelayCommand<RecentFocusSettingsViewModel?> DeleteCommand { get; }
}
