using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
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
    private readonly string LastUsedChannelFilterThisSession = nameof(LastUsedChannelFilterThisSession);
    private readonly IChannelService _channelService;
    private readonly ITelemetry _telemetry;
    private readonly IPageCache _pageCache;
    private readonly IAssetRowVmFactory _assetRowVmFactory;
    private readonly ICategoryService _categoryService;
    private readonly ICategoryVmFactory _categoryVmFactory;
    private readonly IChannelVmFactory _channelVmFactory;
    private readonly IRuntimeMemoryStore _runtimeMemoryStore;
    private readonly SemaphoreSlim _filteredChannelsLock = new(1, 1);

    public EventHandler<ChannelViewModel>? GridVideoPlayed;

    public ChannelsPageViewModel(
        IChannelService channelService,
        ITelemetry telemetry,
        IPageCache pageCache,
        IAssetRowVmFactory assetRowVmFactory,
        ICategoryService categoryService,
        ICategoryVmFactory categoryVmFactory,
        IChannelVmFactory channelVmFactory,
        IRuntimeMemoryStore runtimeMemoryStore)
    {
        _channelService = channelService;
        _telemetry = telemetry;
        _pageCache = pageCache;
        _assetRowVmFactory = assetRowVmFactory;
        _categoryService = categoryService;
        _categoryVmFactory = categoryVmFactory;
        _channelVmFactory = channelVmFactory;
        _runtimeMemoryStore = runtimeMemoryStore;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsPaneVisible))]
    private ChannelViewModel? _selectedChannel;

    public bool DetailsPaneVisible => SelectedChannel is not null;

    public ObservableCollection<ChannelRowViewModel> Rows { get; } = [];

    public ObservableCollection<CategoryViewModel> CategoryFilters { get; } = [];

    public ObservableCollection<ChannelViewModel> FilteredChannels { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredListVisible))]
    private CategoryViewModel? _selectedFilter;

    public bool FilteredListVisible => SelectedFilter is not null;

    [ObservableProperty]
    private bool _loadingChannels;

    public async Task InitializeAsync(string? launchArgs, CancellationToken ct)
    {
        LoadingChannels = true;
        ct.ThrowIfCancellationRequested();

        string? lastUsedFilter = _runtimeMemoryStore.Get<string>(LastUsedChannelFilterThisSession);
        IReadOnlyList<Category> categories = await _categoryService.GetCategoriesAsync([CategorySupportedPage.Channel], ct);
        foreach (Category category in categories)
        {
            CategoryViewModel categoryVm = _categoryVmFactory.Create(category);
            CategoryFilters.Add(categoryVm);
            if (lastUsedFilter == categoryVm.Model.Id)
            {
                SelectedFilter = categoryVm;
            }
        }

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
        CategoryFilters.Clear();
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

    async partial void OnSelectedFilterChanged(CategoryViewModel? oldValue, CategoryViewModel? newValue)
    {
        if (oldValue is { })
        {
            oldValue.IsSelected = false;
        }

        if (newValue is { })
        {
            newValue.IsSelected = true;
            _runtimeMemoryStore.Set(LastUsedChannelFilterThisSession, newValue.Model.Id);
            await UpdateFilteredSoundsAsync(newValue);
            _telemetry.TrackEvent(TelemetryConstants.ChannelFilterClicked, new Dictionary<string, string>
            {
                { "filter", newValue.Model.Id }
            });
        }
    }

    private async Task UpdateFilteredSoundsAsync(CategoryViewModel categoryVm)
    {
        await _filteredChannelsLock.WaitAsync();
        FilteredChannels.Clear();
        IReadOnlyList<Channel> newSounds = await _channelService.GetChannelsAsync(categoryId: categoryVm.Model.Id);
        List<Task> tasks = new(newSounds.Count);
        List<ChannelViewModel> vmList = [];
        foreach (Channel sound in newSounds)
        {
            ChannelViewModel? channelVm = _channelVmFactory.Create(sound, ViewDetailsCommand, PlayChannelCommand);
            if (channelVm is not null)
            {
                tasks.Add(channelVm.InitializeAsync());
                vmList.Add(channelVm);

                if (LoadingChannels)
                {
                    LoadingChannels = false;
                }
            }
        }

        foreach (ChannelViewModel vm in vmList.OrderBy(x => x.Name))
        {
            FilteredChannels.Add(vm);
        }

        await Task.WhenAll(tasks);
        _ = _filteredChannelsLock.Release();
    }


    [RelayCommand]
    private void ClearFilterSelection()
    {
        SelectedFilter = null;
        _runtimeMemoryStore.Set<string?>(LastUsedChannelFilterThisSession, null);
        _telemetry.TrackEvent(TelemetryConstants.ChannelFilterCleared);
    }
}
