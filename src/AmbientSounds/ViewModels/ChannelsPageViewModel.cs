using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelsPageViewModel : ObservableObject
{
    private readonly IChannelService _channelService;
    private readonly ChannelVmFactory _channelFactory;
    private readonly ITelemetry _telemetry;

    public EventHandler<ChannelViewModel>? GridVideoPlayed;

    public ChannelsPageViewModel(
        IChannelService channelService,
        ChannelVmFactory channelFactory,
        ITelemetry telemetry)
    {
        _channelService = channelService;
        _channelFactory = channelFactory;
        _telemetry = telemetry;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsPaneVisible))]
    private ChannelViewModel? _selectedChannel;

    public bool DetailsPaneVisible => SelectedChannel is not null;

    public ObservableCollection<ChannelViewModel> Channels { get; } = [];

    [ObservableProperty]
    private bool _loadingChannels;

    /// <param name="newChannelIds">List of channel IDs that are deemed new. These channels will be highlighted on page navigation.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task InitializeAsync(IReadOnlyList<string> newChannelIds, CancellationToken ct)
    {
        LoadingChannels = true;
        ct.ThrowIfCancellationRequested();

        var channels = await _channelService.GetChannelsAsync();

        ct.ThrowIfCancellationRequested();
        var tasks = new List<Task>();
        foreach (var c in channels)
        {
            ct.ThrowIfCancellationRequested();

            if (_channelFactory.Create(c, ViewDetailsCommand, PlayChannelCommand, newChannelIds.Contains(c.Id)) is { } vm)
            {
                tasks.Add(vm.InitializeAsync());
                Channels.Add(vm);
                LoadingChannels = false;

                if (vm.Id == _channelService.MostRecentChannelDetailsViewed)
                {
                    SelectedChannel = vm;
                }
            }
        }

        ct.ThrowIfCancellationRequested();

        if (SelectedChannel is null
            && newChannelIds is [string id]
            && Channels.FirstOrDefault(x => x.Id == id) is { } newChannel)
        {
            ViewDetails(newChannel);
        }

        await Task.WhenAll(tasks);
    }

    public void Uninitialize()
    {
        foreach (var channel in Channels)
        {
            channel.Uninitialize();
        }

        Channels.Clear();
    }

    [RelayCommand]
    private void ViewDetails(ChannelViewModel? vmToSelect)
    {
        SelectedChannel = vmToSelect;
        _telemetry.TrackEvent(TelemetryConstants.ChannelDetailsClicked, new Dictionary<string, string>
        {
            { "name", vmToSelect?.Channel.Name ?? string.Empty }
        });
    }

    [RelayCommand]
    private async Task PlayChannelAsync(ChannelViewModel? vm)
    {
        if (vm?.Channel is not Channel channel)
        {
            return;
        }

        if (channel.Type is ChannelType.Videos)
        {
            GridVideoPlayed?.Invoke(this, vm);
        }

        await _channelService.PlayChannelAsync(channel);

        _telemetry.TrackEvent(TelemetryConstants.ChannelPlayed, new Dictionary<string, string>
        {
            { "name", channel.Name }
        });
    }

    [RelayCommand]
    private void CloseDetails()
    {
        _telemetry.TrackEvent(TelemetryConstants.ChannelDetailsClosed, new Dictionary<string, string>
        {
            { "name", SelectedChannel?.Channel.Name ?? string.Empty }
        });

        SelectedChannel = null;
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? value)
    {
        _channelService.MostRecentChannelDetailsViewed = value?.Id;
    }
}
