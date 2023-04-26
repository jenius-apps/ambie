using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbientSounds.Models;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

#nullable enable

namespace AmbientSounds.Services;

/// <summary>
/// Class for getting audio device information.
/// </summary>
public class AudioDeviceService : IAudioDeviceService
{
    private readonly ILocalizer _localizer;
    private readonly ITelemetry _telemetry;

    public AudioDeviceService(
        ILocalizer localizer,
        ITelemetry telemetry)
    {
        _localizer = localizer;
        _telemetry = telemetry;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AudioDeviceDescriptor>> GetAudioDeviceDescriptorsAsync()
    {
        DeviceInformationCollection? outputDevices;

        try
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
        }
        catch (Exception e)
        {
            _telemetry.TrackError(e);
            outputDevices = null;
        }

        List<AudioDeviceDescriptor>? list = outputDevices?
            .Select(x => new AudioDeviceDescriptor { DeviceId = x.Id, DeviceName = x.Name})
            .ToList();

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
