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
        Guide guide,
        IAsyncRelayCommand<GuideViewModel?> download,
        IAsyncRelayCommand<GuideViewModel?> delete,
        IAsyncRelayCommand<GuideViewModel?> play,
        IRelayCommand<GuideViewModel?> pause,
        IAssetLocalizer assetLocalizer,
        Progress<double>? progress = null)
    {
        Guide = guide;
        IsDownloaded = guide.IsDownloaded;
        DownloadCommand = download;
        DeleteCommand = delete;
        PlayCommand = play;
        PauseCommand = pause;
        Name = assetLocalizer.GetLocalName(guide);

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
    private bool _isDownloaded;

    [ObservableProperty]
    private bool _isPlaying;

    public bool ProgressRingVisible => Loading || DownloadProgressVisible;

    public Guide Guide { get; }

    public Progress<double> DownloadProgress { get; }

    public string Name { get; }

    public string PreviewText { get; } = "This is a preview";

    public string ImagePath { get; } = "https://images.unsplash.com/photo-1617354161552-cce2e5b27f79?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=640&q=80";

    public IAsyncRelayCommand<GuideViewModel?> DownloadCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> DeleteCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> PlayCommand { get; }

    public IRelayCommand<GuideViewModel?> PauseCommand { get; }

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
