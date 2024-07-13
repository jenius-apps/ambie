using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelsPageViewModel : ObservableObject
{
    private readonly IChannelService _channelService;
    private readonly ISoundVmFactory _soundVmFactory;

    public ChannelsPageViewModel(
        IChannelService channelService,
        ISoundVmFactory soundVmFactory)
    {
        _channelService = channelService;
        _soundVmFactory = soundVmFactory;
    }

    public ObservableCollection<OnlineSoundViewModel> Channels { get; } = [];

    public async Task InitializeAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var channels = await _channelService.GetChannelsAsync();

        ct.ThrowIfCancellationRequested();
        var tasks = new List<Task>();
        foreach (var c in channels)
        {
            ct.ThrowIfCancellationRequested();

            if (_soundVmFactory.GetOnlineSoundVm(c) is { } vm)
            {
                tasks.Add(vm.LoadCommand.ExecuteAsync(null));
                Channels.Add(vm);
            }
        }

        ct.ThrowIfCancellationRequested();

        await Task.WhenAll(tasks);
    }

    public void Uninitialize()
    {
        Channels.Clear();
    }
}
