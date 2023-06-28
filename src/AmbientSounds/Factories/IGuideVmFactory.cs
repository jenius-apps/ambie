using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AmbientSounds.Factories;

public interface IGuideVmFactory
{
    GuideViewModel Create(
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> downloadCommand,
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        IAsyncRelayCommand<GuideViewModel?> playCommand,
        IRelayCommand<GuideViewModel?> stopCommand,
        IAsyncRelayCommand purchaseCommand,
        Progress<double>? downloadProgress = null);
}