using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class GuideService : IGuideService
{
    private readonly ISoundCache _soundcache;
    private readonly IAssetLocalizer _assetLocalizer;

    public GuideService(
        ISoundCache soundCache,
        IAssetLocalizer assetLocalizer)
    {
        _soundcache = soundCache;
        _assetLocalizer = assetLocalizer;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetGuidesAsync(IReadOnlyList<string> guideIds)
    {
        if (guideIds is { Count: 0 })
        {
            return Array.Empty<Guide>();
        }

        return await _soundcache.GetOnlineSoundsAsync<Guide>(guideIds);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetSuggestedSoundMixesAsync(Guide g)
    {
        if (g.SuggestedBackgroundSounds.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        Dictionary<string, Sound?> soundMap = new();
        List<string[]> splitIdList = new(g.SuggestedBackgroundSounds.Count);

        // Flatten the id list so we can batch the fetch call.
        foreach (var idList in g.SuggestedBackgroundSounds)
        {
            string[] list = idList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in list)
            {
                soundMap.Add(id, null);
            }
            splitIdList.Add(list);
        }

        // Get sound already installed
        var offlineSounds = await _soundcache.GetInstalledSoundsAsync(soundMap.Keys);
        foreach (var offlineSound in offlineSounds)
        {
            soundMap[offlineSound.Id] = offlineSound;
        }

        // Perform a batch fetch call for all ids that are not installed.
        IReadOnlyList<Sound> onlineSounds = await _soundcache.GetOnlineSoundsAsync<Sound>(
            soundMap.Where(x => x.Value is null).Select(x => x.Key).ToArray());

        foreach (var onlineSound in onlineSounds)
        {
            soundMap[onlineSound.Id] = onlineSound;
        }

        List<Sound> suggestedMixes = new(g.SuggestedBackgroundSounds.Count);

        // Generate sound mix objects for each group of IDs.
        foreach (string[] split in splitIdList)
        {
            IEnumerable<Sound> sounds = soundMap
                .Where(x => split.Contains(x.Key) && x.Value is not null)
                .Select(x => x.Value)
                .Cast<Sound>();

            var tempMix = new Sound
            {
                IsMix = true,
                Id = Guid.NewGuid().ToString(),
                Name = string.Join(", ", sounds.Select(_assetLocalizer.GetLocalName)),
                SoundIds = split
            };

            suggestedMixes.Add(tempMix);
        }

        return suggestedMixes;
    }
}
