using CommunityToolkit.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services
{
    public class AppServiceController
    {
        private readonly IMixMediaPlayerService _player;
        private readonly DispatcherQueue _dispatcherQueue;

        public AppServiceController(
            IMixMediaPlayerService player)
        {
            Guard.IsNotNull(player, nameof(player));

            _player = player;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public async Task ProcessRequest(AppServiceRequest request)
        {
            ValueSet message = request.Message;
            string result = "";

            if (message["command"] is string command)
            {
                switch (command)
                {
                    case "pause":
                        _dispatcherQueue.TryEnqueue(_player.Pause);
                        result = "Ok";
                        break;
                    case "resume":
                        _dispatcherQueue.TryEnqueue(_player.Play);
                        result = "Ok";
                        break;
                    default:
                        result = "Unknown command: " + command;
                        break;
                }
            }
            else if (message["state"] is string state)
            {
                switch (state)
                {
                    case "playback":
                        result = _player.PlaybackState.ToString();
                        break;
                    case "volume":
                        result = _player.GlobalVolume.ToString();
                        break;
                }
            }
            else
            {
                result = "No valid keys found";
            }

            ValueSet returnMessage = new();
            returnMessage.Add("result", result);
            await request.SendResponseAsync(returnMessage);
        }
    }
}
