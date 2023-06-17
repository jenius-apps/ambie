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
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        IAsyncRelayCommand<GuideViewModel?> playCommand,
        IRelayCommand<GuideViewModel?> pauseCommand,
        Progress<double>? downloadProgress = null);
}