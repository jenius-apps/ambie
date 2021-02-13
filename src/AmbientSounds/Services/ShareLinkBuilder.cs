using Microsoft.Toolkit.Diagnostics;
using System.Collections.Generic;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for building the share sounds link.
    /// </summary>
    public class ShareLinkBuilder : IShareLinkBuilder
    {
        private readonly IMixMediaPlayerService _player;

        public ShareLinkBuilder(
            IMixMediaPlayerService mixMediaPlayerService)
        {
            Guard.IsNotNull(mixMediaPlayerService, nameof(mixMediaPlayerService));
            _player = mixMediaPlayerService;
        }

        /// <inheritdoc/>
        public string GetLink()
        {
            IList<string> soundIds = _player.GetActiveIds();
            var encodedIds = new string[soundIds.Count];
            for (int i = 0; i < soundIds.Count; i++)
            {
                encodedIds[i] = GuidEncoder.Encode(soundIds[i]);
            }
            return $"https://ambie-app.azurewebsites.net/play?sounds={string.Join(",", encodedIds)}";
        }
    }
}
