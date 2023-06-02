using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class FocusHistoryModuleViewModel : ObservableObject
{
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly ITelemetry _telemetry;
    private readonly IDialogService _dialogService;
    private readonly IDispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private bool _loading;

    [ObservableProperty]
    private bool _placeholderVisible;

    public FocusHistoryModuleViewModel(
        IFocusHistoryService focusHistoryService,
        ITelemetry telemetry,
        IDialogService dialogService,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(focusHistoryService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(dialogService);
        Guard.IsNotNull(dispatcherQueue);

        _focusHistoryService = focusHistoryService;
        _telemetry = telemetry;
        _dialogService = dialogService;
        _dispatcherQueue = dispatcherQueue;
    }

    public ObservableCollection<FocusHistoryViewModel> Items { get; } = new();

    public async Task InitializeAsync()
    {
        _focusHistoryService.HistoryAdded += OnHistoryAdded;
        Items.Clear();

        Loading = true;
        var recent = await _focusHistoryService.GetRecentAsync();
        await Task.Delay(300);
        Loading = false;
        foreach (var focusHistory in recent.OrderByDescending(x => x.StartUtcTicks))
        {
            Items.Add(new FocusHistoryViewModel(focusHistory));
        }

        UpdatePlaceholder();
    }

    public void Uninitialize()
    {
        Items.Clear();
        _focusHistoryService.HistoryAdded -= OnHistoryAdded;
    }

    private void UpdatePlaceholder() => PlaceholderVisible = Items.Count == 0;

    private void OnHistoryAdded(object sender, FocusHistory? history)
    {
        if (history is FocusHistory f)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                Items.Insert(0, new FocusHistoryViewModel(f));
                UpdatePlaceholder();
            });
        }
    }

    [RelayCommand]
    private async Task ViewDetailsAsync(FocusHistoryViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        await _dialogService.OpenHistoryDetailsAsync(vm);

        _telemetry.TrackEvent(TelemetryConstants.FocusHistoryClicked, new Dictionary<string, string>
        {
            { "index", Items.IndexOf(vm).ToString() },
            { "percentComplete", vm.PercentComplete }
        });
    }
}
