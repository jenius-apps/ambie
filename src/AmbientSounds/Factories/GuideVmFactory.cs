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
    private readonly ConcurrentDictionary<string, GuideViewModel> _onlineGuideVmCache = new();

    public GuideVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GuideViewModel GetOrCreate(Guide guide)
    {
        if (_onlineGuideVmCache.TryGetValue(guide.Id, out var vm))
        {
            return vm;
        }

        var newVm = new GuideViewModel(
            guide,
            _serviceProvider.GetRequiredService<IAssetLocalizer>(),
            _serviceProvider.GetRequiredService<IDialogService>());

        _onlineGuideVmCache.TryAdd(guide.Id, newVm);
        return newVm;
    }
}
