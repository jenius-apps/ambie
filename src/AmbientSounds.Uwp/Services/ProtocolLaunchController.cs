using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Diagnostics;
using System;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services
{
    public class ProtocolLaunchController
    {
        private readonly IMixMediaPlayerService _player;
        private readonly INavigator _navigator;
        private readonly DispatcherQueue _dispatcherQueue;

        private const string CompactKey = "compact";
        private const string AutoPlayKey = "autoplay";

        public ProtocolLaunchController(
            IMixMediaPlayerService player,
            INavigator navigator)
        {
            Guard.IsNotNull(player, nameof(player));

            _player = player;
            _navigator = navigator;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public void ProcessProtocolArguments(string arguments)
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
