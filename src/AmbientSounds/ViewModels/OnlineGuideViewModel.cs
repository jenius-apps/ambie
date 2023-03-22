using AmbientSounds.Models;
using AmbientSounds.Services;
using Humanizer;
using Humanizer.Localisation;
using JeniusApps.Common.Tools;
using System;

namespace AmbientSounds.ViewModels;

public partial class OnlineGuideViewModel : OnlineSoundViewModel
{
    public OnlineGuideViewModel(
        Guide g,
        IDownloadManager downloadManager,
        ISoundService soundService,
        ITelemetry telemetry,
        IPreviewService previewService,
        IIapService iapService,
        IDialogService dialogService,
        IAssetLocalizer assetLocalizer,
        IMixMediaPlayerService mixMediaPlayerService,
        IUpdateService updateService,
        ILocalizer localizer)
        : base(g,
            downloadManager,
            soundService,
            telemetry,
            previewService,
            iapService,
            dialogService,
            assetLocalizer,
            mixMediaPlayerService,
            updateService,
            localizer)
    {
        Minutes = TimeSpan.FromMinutes(g.MinutesLength).Humanize(maxUnit: TimeUnit.Minute);
        PreviewText = string.Join(" • ", new string[]
        {
            Minutes,
            Description
        });
    }

    public string Minutes { get; }

    public string PreviewText { get; }
}
