using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for performing sound mix operations.
    /// </summary>
    public class SoundMixService : ISoundMixService
    {
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IMixMediaPlayerService _player;
        private readonly string[] _namePlaceholders = new string[] { "🎵", "🎼", "🎧", "🎶" };

        public SoundMixService(
            ISoundDataProvider soundDataProvider,
            IMixMediaPlayerService player)
        {
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(player, nameof(player));

            _soundDataProvider = soundDataProvider;
            _player = player;
        }

        /// <inheritdoc/>
        public bool IsMixPlaying(string mixId)
        {
            return _player.CurrentMixId == mixId;
        }

        /// <inheritdoc/>
        public async Task<string> SaveMixAsync(IList<Sound> sounds, string name = "")
        {
            if (sounds is null || sounds.Count <= 1)
            {
                return "";
            }

            var mix = new Sound()
            {
                Id = Guid.NewGuid().ToString(),
                IsMix = true,
                Name = string.IsNullOrWhiteSpace(name) ? RandomName() : name.Trim(),
                SoundIds = sounds.Select(static x => x.Id).ToArray(),
                ImagePaths = sounds.Select(static x => x.ImagePath).ToArray()
            };

            await _soundDataProvider.AddLocalSoundAsync(mix);
            return mix.Id;
        }

        /// <inheritdoc/>
        public async Task<bool> LoadMixAsync(Sound mix)
        {
            if (mix?.SoundIds is null || !mix.IsMix) return false;

            // save instance of id
            // since RemoveAll will reset the id.
            var previousMixId = _player.CurrentMixId;

            // if the mix we're trying to play was
            // the same as the previous, return now
            // since we don't want to play it again.
            if (!string.IsNullOrWhiteSpace(previousMixId) && 
                previousMixId == mix.Id)
            {
                return false;
            }

            _player.RemoveAll();

            var sounds = await _soundDataProvider.GetSoundsAsync(soundIds: mix.SoundIds);
            if (sounds is not null && sounds.Count == mix.SoundIds.Length)
            {
                foreach (var soundId in mix.SoundIds)
                {
                    var sound = sounds.FirstOrDefault(x => x.Id == soundId);
                    await _player.ToggleSoundAsync(sound, parentMixId: mix.Id);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task ReconstructMixesAsync(IList<Sound> dehydratedMixes)
        {
            if (dehydratedMixes is null || dehydratedMixes.Count == 0)
            {
                return;
            }

            var allSounds = await _soundDataProvider.GetSoundsAsync();
            var allSoundIds = allSounds.Select(static x => x.Id);

            foreach (var soundMix in dehydratedMixes)
            {
                if (allSoundIds.Contains(soundMix.Id) || 
                    soundMix.SoundIds is null || 
                    soundMix.SoundIds.Length == 0)
                {
                    continue;
                }

                var soundsForThisMix = allSounds.Where(x => soundMix.SoundIds.Contains(x.Id)).ToList();

                var hydratedMix = new Sound()
                {
                    Id = soundMix.Id,
                    Name = soundMix.Name,
                    IsMix = true,
                    SoundIds = soundsForThisMix.Select(static x => x.Id).ToArray(),
                    ImagePaths = soundsForThisMix.Select(static x => x.ImagePath).ToArray()
                };

                await _soundDataProvider.AddLocalSoundAsync(hydratedMix);
            }
        }

        private string RandomName()
        {
            var rand = new Random();
            var result = rand.Next(4);
            return _namePlaceholders[result];
        }
    }
}
