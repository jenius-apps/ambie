using AmbientSounds.Cache;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ShareService : IShareService
{
    private readonly IShareDetailCache _shareDetailCache;
    private readonly string _baseShareUrl;

    public event EventHandler<IReadOnlyList<string>>? ShareRequested;

    public event EventHandler? ShareFailed;

    public ShareService(
        IShareDetailCache shareDetail,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(shareDetail);

        _shareDetailCache = shareDetail;
        _baseShareUrl = appSettings.ShareUrl;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string>? FailedSoundIds { get; private set; }

    /// <inheritdoc/>
    public async Task ProcessShareRequestAsync(string shareId)
    {
        Guard.IsNotNull(shareId);

        IReadOnlyList<string> soundIds = await GetSoundIdsAsync(shareId);
        if (soundIds.Count == 0)
        {
            return;
        }

        ShareRequested?.Invoke(this, soundIds);
    }

    /// <inheritdoc/>
    public void LogShareFailed(IReadOnlyList<string> soundIds)
    {
        if (soundIds.Count > 0)
        {
            FailedSoundIds = soundIds;
            ShareFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetShareUrlAsync(IReadOnlyList<string> soundIds)
    {
        ShareDetail? shareDetail = await _shareDetailCache.GetShareDetailAsync(soundIds);
        return shareDetail is { Id: string id } 
            ? $"{_baseShareUrl}?id={id}" 
            : string.Empty;
    }

    private async Task<IReadOnlyList<string>> GetSoundIdsAsync(string shareId)
    {
        ShareDetail? detail = await _shareDetailCache.GetShareDetailAsync(shareId);
        if (string.IsNullOrEmpty(detail?.SoundIdComposite))
        {
            return Array.Empty<string>();
        }

        return detail!.SplitCompositeId();
    }
}
