using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Web;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for deciphering url and performing
    /// its actions.
    /// </summary>
    public class LinkProcessor : ILinkProcessor
    {
        private readonly ISoundMixService _soundMixService;

        public LinkProcessor(ISoundMixService soundMixService)
        {
            Guard.IsNotNull(soundMixService, nameof(soundMixService));

            _soundMixService = soundMixService;
        }

        /// <inheritdoc/>
        public async void Process(Uri uri)
        {
            if (uri?.Query == null)
            {
                return;
            }

            var queryString = HttpUtility.ParseQueryString(uri.Query);
            var sounds = queryString["sounds"] ?? "";
            if (string.IsNullOrWhiteSpace(sounds))
            {
                return;
            }
            string[] list = sounds.Split(',');

            for (int x = 0; x < list.Length; x++)
            {
                list[x] = GuidEncoder.Decode(list[x]).ToString();
            }

            Sound tempSoundMix = new()
            {
                SoundIds = list,
                IsMix = true
            };

            var allLoaded = await _soundMixService.LoadMixAsync(tempSoundMix);

            if (!allLoaded)
            {
                // trigger dialog
            }
        }
    }
}
