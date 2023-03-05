using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmbientSounds.Models;
using JeniusApps.Common.Tools;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

namespace AmbientSounds.Services
{
    public class AudioDeviceService : IAudioDeviceService
    {
        private readonly ILocalizer _localizer;

        public AudioDeviceService(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<AudioDeviceDescriptor>> GetAudioDeviceDescriptorsAsync()
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);

            var list = outputDevices.Select(x => new AudioDeviceDescriptor
            {
                DeviceId = x.Id,
                DeviceName = x.Name
            }).ToList();

            if (list is null)
            {
                list = new List<AudioDeviceDescriptor>()
                {
                    GetDefaultAudioDeviceDescriptor()
                };
            }
            else
            {
                list.Insert(0, GetDefaultAudioDeviceDescriptor());
            }

            return list;
        }

        /// <inheritdoc/>
        public AudioDeviceDescriptor GetDefaultAudioDeviceDescriptor()
        {
            return new AudioDeviceDescriptor
            {
                DeviceId = string.Empty,
                DeviceName = _localizer.GetString("Default")
            };
        }
    }
}
