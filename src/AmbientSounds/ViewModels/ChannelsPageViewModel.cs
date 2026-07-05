using AmbientSounds.Cache;
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
    private readonly ITelemetry _telemetry;
    private readonly IPageCache _pageCache;
    private readonly IAssetRowVmFactory _assetRowVmFactory;

    public EventHandler<ChannelViewModel>? GridVideoPlayed;

    public ChannelsPageViewModel(
        IChannelService channelService,
        ITelemetry telemetry,
        IPageCache pageCache,
        IAssetRowVmFactory assetRowVmFactory)
    {
        _channelService = channelService;
        _telemetry = telemetry;
        _pageCache = pageCache;
        _assetRowVmFactory = assetRowVmFactory;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsPaneVisible))]
    private ChannelViewModel? _selectedChannel;

    public bool DetailsPaneVisible => SelectedChannel is not null;

    public ObservableCollection<ChannelRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private bool _loadingChannels;

    public async Task InitializeAsync(string? launchArgs, CancellationToken ct)
    {
        LoadingChannels = true;
        ct.ThrowIfCancellationRequested();

        IReadOnlyList<AssetRow> channelRows = await _pageCache.GetChannelPageRowsAsync(ct);

        foreach (AssetRow row in channelRows)
        {
            ct.ThrowIfCancellationRequested();
            if (_assetRowVmFactory.CreateChannelRowVm(row, ViewDetailsCommand, PlayChannelCommand) is not ChannelRowViewModel vm)
            {
                continue;
            }

            await vm.LoadAsync(launchArgs, ct);
            Rows.Add(vm);

            if (LoadingChannels)
            {
                LoadingChannels = false;
            }

            if (SelectedChannel is null
                && vm.Channels.FirstOrDefault(x => x.Id == _channelService.MostRecentChannelDetailsViewed) is { } selectedChannel)
            {
                SelectedChannel = selectedChannel;
            }
        }
    }

    public void Uninitialize()
    {
        foreach (ChannelRowViewModel row in Rows)
        {
            row.Uninitialize();
        }

        Rows.Clear();
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
