using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Diagnostics;
using System;

#nullable enable

namespace AmbientSounds.Services
{
    public class ProtocolLaunchController
    {
        private readonly IMixMediaPlayerService _player;
        private readonly INavigator _navigator;

        private const string CompactKey = "compact";
        private const string AutoPlayKey = "autoPlay";

        public ProtocolLaunchController(
            IMixMediaPlayerService player,
            INavigator navigator)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(navigator, nameof(navigator));

            _player = player;
            _navigator = navigator;
        }

        public void ProcessLaunchProtocolArguments(string arguments)
        {
            var query = QueryString.Parse(arguments);
            query.TryGetValue(CompactKey, out var isCompact);
            query.TryGetValue(AutoPlayKey, out var isAutoPlay);

            if (!string.IsNullOrEmpty(isCompact) && Convert.ToBoolean(isCompact))
            {
                // Enter compact view.
                _navigator.ToCompact();
            }

            if(!string.IsNullOrEmpty(isAutoPlay) && Convert.ToBoolean(isAutoPlay))
            {
                // Auto play music.
                _player.Play();
            }
        }
    }
}
