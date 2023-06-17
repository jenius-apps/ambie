using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AmbientSounds.Factories;

public class GuideVmFactory : IGuideVmFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GuideVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GuideViewModel GetOrCreate(
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> downloadCommand,
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        IAsyncRelayCommand<GuideViewModel?> playCommand,
        IRelayCommand<GuideViewModel?> pauseCommand,
        Progress<double>? downloadProgress = null)
    {
        var newVm = new GuideViewModel(
            guide,
            downloadCommand,
            deleteCommand,
            playCommand,
            pauseCommand,
            _serviceProvider.GetRequiredService<IAssetLocalizer>(),
            downloadProgress);

        return newVm;
    }
}
