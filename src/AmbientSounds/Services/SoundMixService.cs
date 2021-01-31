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
        public async Task<string> SaveMixAsync(IList<Sound> sounds)
        {
            if (sounds == null || sounds.Count <= 1)
            {
                return "";
            }

            var mix = new Sound()
            {
                Id = Guid.NewGuid().ToString(),
                IsMix = true,
                Name = RandomName(),
                SoundIds = sounds.Select(x => x.Id).ToArray(),
                ImagePaths = sounds.Select(x => x.ImagePath).ToArray()
            };

            await _soundDataProvider.AddLocalSoundAsync(mix);
            return mix.Id;
        }

        private string RandomName()
        {
            var rand = new Random();
            var result = rand.Next(4);
            return _namePlaceholders[result];
        }

        /// <inheritdoc/>
        public async Task LoadMixAsync(Sound mix)
        {
            if (!mix.IsMix) return;

            // save instance of id
            // since RemoveAll will reset the id.
            var previousMixId = _player.CurrentMixId;
            _player.RemoveAll();

            // if the mix we're trying to play was
            // the same as the previous, return now
            // since we don't want to play it again.
            if (previousMixId == mix.Id)
            {
                return;
            }

            var sounds = await _soundDataProvider.GetSoundsAsync(mix.SoundIds);
            if (sounds != null)
            {
                foreach (var s in sounds)
                {
                    await _player.ToggleSoundAsync(s, parentMixId: mix.Id);
                }
            }
        }
    }
}
