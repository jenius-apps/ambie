﻿using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public ChannelsPageViewModel(
        IChannelService channelService,
        ChannelVmFactory channelFactory)
    {
        _channelService = channelService;
        _channelFactory = channelFactory;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsPaneVisible))]
    private ChannelViewModel? _selectedChannel;

    public bool DetailsPaneVisible => SelectedChannel is not null;

    public ObservableCollection<ChannelViewModel> Channels { get; } = [];

    public async Task InitializeAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var channels = await _channelService.GetChannelsAsync();

        ct.ThrowIfCancellationRequested();
        var tasks = new List<Task>();
        foreach (var c in channels.OrderBy(x => x.Key))
        {
            ct.ThrowIfCancellationRequested();

            if (_channelFactory.Create(c.Value, ViewDetailsCommand) is { } vm)
            {
                tasks.Add(vm.InitializeAsync());
                Channels.Add(vm);

                if (vm.Id == _channelService.MostRecentChannelDetailsViewed)
                {
                    SelectedChannel = vm;
                }
            }
        }

        ct.ThrowIfCancellationRequested();

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
    }

    [RelayCommand]
    private void CloseDetails()
    {
        SelectedChannel = null;
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? value)
    {
        _channelService.MostRecentChannelDetailsViewed = value?.Id;
    }
}