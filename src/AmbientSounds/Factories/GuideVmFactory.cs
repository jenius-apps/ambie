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
    private readonly ConcurrentDictionary<string, GuideViewModel> _onlineGuideVmCache = new();

    public GuideVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GuideViewModel GetOrCreate(
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> downloadCommand,
        IAsyncRelayCommand<GuideViewModel?> deleteCommand,
        IAsyncRelayCommand<GuideViewModel?> playCommand,
        IAsyncRelayCommand<GuideViewModel?> pauseCommand,
        Progress<double>? downloadProgress = null)
    {
        if (_onlineGuideVmCache.TryGetValue(guide.Id, out var vm))
        {
            return vm;
        }

        var newVm = new GuideViewModel(
            guide,
            downloadCommand,
            deleteCommand,
            playCommand,
            pauseCommand,
            _serviceProvider.GetRequiredService<IAssetLocalizer>(),
            downloadProgress);

        _onlineGuideVmCache.TryAdd(guide.Id, newVm);
        return newVm;
    }
}
