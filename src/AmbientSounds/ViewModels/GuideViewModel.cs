using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class GuideViewModel : ObservableObject
{
    public GuideViewModel(
        Guide onlineGuide,
        IAsyncRelayCommand<GuideViewModel?> download,
        IAsyncRelayCommand<GuideViewModel?> delete,
        IAsyncRelayCommand<GuideViewModel?> play,
        IRelayCommand<GuideViewModel?> pause,
        IAsyncRelayCommand purchase,
        IAssetLocalizer assetLocalizer,
        Progress<double>? progress = null)
    {
        OnlineGuide = onlineGuide;
        DownloadCommand = download;
        DeleteCommand = delete;
        PlayCommand = play;
        PauseCommand = pause;
        PurchaseCommand = purchase;
        Name = assetLocalizer.GetLocalName(onlineGuide);
        PreviewText = $"{onlineGuide.MinutesLength}m {FocusConstants.DotSeparator} {assetLocalizer.GetLocalDescription(onlineGuide)}";
        ImagePath = onlineGuide.ImagePath;

        DownloadProgress = progress ?? new();
        DownloadProgress.ProgressChanged += OnProgressChanged;
    }

    [ObservableProperty]
    private double _downloadProgressValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _loading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _downloadProgressVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlaybackButtonsVisible))]
    private bool _isDownloaded;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlaybackButtonsVisible))]
    private bool _isOwned;

    public bool ProgressRingVisible => Loading || DownloadProgressVisible;

    public Guide OnlineGuide { get; }

    public Progress<double> DownloadProgress { get; }

    public string Name { get; }

    public string PreviewText { get; }

    public string ImagePath { get; }

    public bool BuyButtonVisible => !IsOwned;

    public bool DownloadButtonVisible => IsOwned && !IsDownloaded;

    public bool PlaybackButtonsVisible => IsOwned && IsDownloaded;

    public IAsyncRelayCommand<GuideViewModel?> DownloadCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> DeleteCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> PlayCommand { get; }

    public IRelayCommand<GuideViewModel?> PauseCommand { get; }

    public IAsyncRelayCommand PurchaseCommand { get; }

    private void OnProgressChanged(object sender, double e)
    {
        if (e <= 0)
        {
            Loading = true;
            DownloadProgressVisible = false;
        }
        else if (e >= 1 && e < 100)
        {
            Loading = false;
            DownloadProgressVisible = true;
        }
        else if (e >= 100)
        {
            IsDownloaded = true;
            DownloadProgressVisible = false;
        }

        DownloadProgressValue = e;
    }

    [RelayCommand]
    private async Task ToggleAsync()
    {
        if (IsPlaying)
        {
            PauseCommand.Execute(this);
        }
        else
        {
            await PlayCommand.ExecuteAsync(this);
        }
    }
}
