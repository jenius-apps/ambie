using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public class SoundEffectsService : ISoundEffectsService
    {
        private readonly ConcurrentDictionary<SoundEffect, string> _pathCache = new();
        private readonly IMediaPlayer _mediaPlayer;
        private readonly Lazy<Task<IReadOnlyList<string>>> _soundEffectsLazy;

        public SoundEffectsService(
            IMediaPlayerFactory mediaPlayerFactory,
            Tools.IAssetsReader assetsReader)
        {
            Guard.IsNotNull(mediaPlayerFactory);
            _mediaPlayer = mediaPlayerFactory.CreatePlayer(disableDefaultSystemControls: true);
            _soundEffectsLazy = new Lazy<Task<IReadOnlyList<string>>>(assetsReader.GetSoundEffectsAsync);
        }

        /// <inheritdoc/>
        public async Task Play(SoundEffect effect)
        {
            if (_pathCache.TryGetValue(effect, out string path))
            {
                _mediaPlayer.SetUriSource(new Uri(path));
                _mediaPlayer.Play();
                return;
            }

            // If path is not cached yet, fetch it now.
            var pathsList = await _soundEffectsLazy.Value;
            string? effectPath = pathsList.Where(x => x.Contains(effect.ToString().ToLower())).FirstOrDefault();

            if (effectPath is not null)
            {
                // Add to cache to optimize future calls.
                _pathCache.TryAdd(effect, effectPath);

                // Don't forget to play the effect :)
                _mediaPlayer.SetUriSource(new Uri(effectPath));
                _mediaPlayer.Play();
            }
        }
    }
}
