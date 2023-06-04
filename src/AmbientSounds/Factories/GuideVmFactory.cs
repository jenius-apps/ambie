using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AmbientSounds.Factories;

public class GuideVmFactory : IGuideVmFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, OnlineGuideViewModel> _onlineGuideVmCache = new();

    public GuideVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public OnlineGuideViewModel GetOrCreate(Guide guide)
    {
        if (_onlineGuideVmCache.TryGetValue(guide.Id, out var vm))
        {
            return vm;
        }

        var newVm = new OnlineGuideViewModel(
            guide,
            _serviceProvider.GetRequiredService<IAssetLocalizer>());

        _onlineGuideVmCache.TryAdd(guide.Id, newVm);
        return newVm;
    }
}
