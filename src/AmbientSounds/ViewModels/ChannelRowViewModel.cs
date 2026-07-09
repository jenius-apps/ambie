using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelRowViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IChannelService _channelService;
    private readonly IChannelVmFactory _vmFactory;
    private readonly AssetRow _row;
    private readonly IRelayCommand<ChannelViewModel>? _viewDetailsCommand;
    private readonly IRelayCommand<ChannelViewModel>? _playCommand;

    public ChannelRowViewModel(
        AssetRow row,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand,
        IRelayCommand<ChannelViewModel>? playCommand,
        IAssetLocalizer assetLocalizer,
        IChannelService channelService,
        IChannelVmFactory vmFactory)
    {
        _row = row;
        _viewDetailsCommand = viewDetailsCommand;
        _playCommand = playCommand;
        _channelService = channelService;
        _vmFactory = vmFactory;
        _assetLocalizer = assetLocalizer;

        Title = _assetLocalizer.GetLocalInfo(row)?.Name ?? row.Id;
    }

    public string Title { get; }

    [ObservableProperty]
    private bool _rowVisible;

    [ObservableProperty]
    private bool _newAnimationVisible;

    public ObservableCollection<ChannelViewModel> Channels { get; } = [];

    public async Task LoadAsync(string? launchArgs, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<Channel>? channels = null;

        try
        {
            channels = await _channelService.GetChannelsAsync(_row.Assets);
        }
        catch { }

        ct.ThrowIfCancellationRequested();

        List<ChannelViewModel> vmList = [];
        if (channels is { Count: > 0 })
        {
            Channels.Clear();
            List<Task> tasks = new(channels.Count);
            foreach (Channel channel in channels)
            {
                ct.ThrowIfCancellationRequested();
                ChannelViewModel? vm = _vmFactory.Create(channel, _viewDetailsCommand, _playCommand);
                if (vm is not null)
                {
                    tasks.Add(vm.InitializeAsync());
                    vmList.Add(vm);
                }
            }

            IEnumerable<ChannelViewModel> sortedVmList = vmList.OrderBy(x => x.Name);

            foreach (ChannelViewModel vm in sortedVmList)
            {
                Channels.Add(vm);
                RowVisible = true;
            }

            await Task.WhenAll(tasks);
        }

        HandleLaunchArgs(launchArgs);
    }

    private void HandleLaunchArgs(string? launchArgs)
    {
        if (launchArgs == LaunchConstants.NewSoundArgument && _row.Id.ToLower() == "new")
        {
            NewAnimationVisible = true;
        }

        // TODO handle individual new channel IDs (maybe)
    }

    public void Uninitialize()
    {
        RowVisible = false;

        foreach (ChannelViewModel s in Channels)
        {
            s.Uninitialize();
        }

        Channels.Clear();
    }
}
