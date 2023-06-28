using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class GuideVmFactory : IGuideVmFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GuideVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GuideViewModel Create(
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> downloadCommand,
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        IAsyncRelayCommand<GuideViewModel?> playCommand,
        IRelayCommand<GuideViewModel?> stopCommand,
        IAsyncRelayCommand purchaseCommand,
        Progress<double>? downloadProgress = null)
    {
        var newVm = new GuideViewModel(
            guide,
            downloadCommand,
            deleteCommand,
            playCommand,
            stopCommand,
            purchaseCommand,
            _serviceProvider.GetRequiredService<IAssetLocalizer>(),
            _serviceProvider.GetRequiredService<IMixMediaPlayerService>(),
            _serviceProvider.GetRequiredService<IDispatcherQueue>(),
            downloadProgress);

        return newVm;
    }
}
