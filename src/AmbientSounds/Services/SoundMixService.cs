using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
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
        private readonly IMixMediaPlayerService _player;
        private readonly ISoundService _soundService;
        private readonly string[] _namePlaceholders = new string[] { "🎵", "🎼", "🎧", "🎶" };

        public SoundMixService(
            ISoundService soundService,
            IMixMediaPlayerService player)
        {
            Guard.IsNotNull(player);
            Guard.IsNotNull(soundService);

            _player = player;
            _soundService = soundService;
        }

        /// <inheritdoc/>
        public bool IsMixPlaying(string mixId)
        {
            return _player.CurrentMixId == mixId;
        }

        /// <inheritdoc/>
        public async Task<string> SaveCurrentMixAsync(string name = "")
        {
            if (!CanSaveCurrentMix())
            {
                return string.Empty;
            }

            var activeTracks = _player.GetSoundIds();
            var sounds = await _soundService.GetLocalSoundsAsync(soundIds: activeTracks);
            var id = await SaveMixAsync(sounds, name);

            if (!string.IsNullOrEmpty(id))
            {
                _player.SetMixId(id);
            }

            return id;
        }

        /// <inheritdoc/>
        public bool CanSaveCurrentMix()
        {
            return string.IsNullOrWhiteSpace(_player.CurrentMixId) && _player.GetSoundIds().Length > 1;
        }

        /// <inheritdoc/>
        public async Task<string> SaveMixAsync(IReadOnlyList<Sound> sounds, string name = "")
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

            await _soundService.AddLocalSoundAsync(mix);
            return mix.Id;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetUnavailableSoundsAsync(Sound mix)
        {
            if (mix?.SoundIds is null || !mix.IsMix)
            {
                return Enumerable.Empty<string>();
            }

            var sounds = await _soundService.GetLocalSoundsAsync(soundIds: mix.SoundIds);
            if (sounds is null)
            {
                return Enumerable.Empty<string>();
            }

            var availableIds = sounds.Select(x => x.Id);
            return mix.SoundIds.Where(id => !availableIds.Contains(id));
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

            var sounds = await _soundService.GetLocalSoundsAsync(soundIds: mix.SoundIds);
            if (sounds is not null && sounds.Count == mix.SoundIds.Length)
            {
                await _player.ToggleSoundsAsync(sounds, mix.Id);
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

            var allSounds = await _soundService.GetLocalSoundsAsync();
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

                await _soundService.AddLocalSoundAsync(hydratedMix);
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
