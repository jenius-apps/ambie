using Microsoft.Toolkit.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System;

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
                    case "play":
                        _dispatcherQueue.TryEnqueue(_player.Play);
                        result = "Ok";
                        break;
                    default:
                        result = "Unknown command: " + command;
                        break;
                }
            }
            else
            {
                result = "No command found";
            }

            ValueSet returnMessage = new ValueSet();
            returnMessage.Add("result", result);
            await request.SendResponseAsync(returnMessage);
        }
    }
}
