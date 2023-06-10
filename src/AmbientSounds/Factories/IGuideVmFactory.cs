using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AmbientSounds.Factories;

public interface IGuideVmFactory
{
    GuideViewModel GetOrCreate(
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> downloadCommand,
        IAsyncRelayCommand<GuideViewModel?> togglePlaybackCommand,
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        Progress<double>? downloadProgress = null);
}