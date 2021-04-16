using AmbientSounds.Constants;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDialogService _dialogService;
        private readonly ITelemetry _telemetry;

        public LinkProcessor(
            ITelemetry telemetry,
            ISoundMixService soundMixService,
            IDialogService dialogService)
        {
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _telemetry = telemetry;
            _soundMixService = soundMixService;
            _dialogService = dialogService;
        }

        /// <inheritdoc/>
        public async void Process(Uri uri)
        {
            if (uri?.Query is null)
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
            bool manuallyPlayed = false;
            int tracksplayed = list.Length;

            if (!allLoaded)
            {
                // show the share result to the user and let them download missing sounds.
                IList<string> soundIdsToPlay = await _dialogService.OpenShareResultsAsync(list);

                if (soundIdsToPlay is not null && soundIdsToPlay.Count > 0)
                {
                    manuallyPlayed = true;
                    tempSoundMix.SoundIds = soundIdsToPlay.ToArray();
                    await _soundMixService.LoadMixAsync(tempSoundMix);
                }

                tracksplayed = soundIdsToPlay is not null ? soundIdsToPlay.Count : 0;
            }

            _telemetry.TrackEvent(TelemetryConstants.ShareReceived, new Dictionary<string, string>()
            {
                { "auto played", allLoaded.ToString() },
                { "manually played", manuallyPlayed.ToString() },
                { "tracks played count", tracksplayed.ToString() }
            });
        }
    }
}
