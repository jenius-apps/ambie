using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AmbientSounds.Models;

namespace AmbientSounds.Services;

/// <summary>
/// Interface for getting audio device information.
/// </summary>
public interface IAudioDeviceService
{
    /// <summary>
    /// Gets all the currently available audio devices that can be used as the endpoint for sound playback.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyList<AudioDeviceDescriptor>> GetAudioDeviceDescriptorsAsync();

    /// <summary>
    /// Gets the descriptor of the default audio device.
    /// </summary>
    /// <returns></returns>
    AudioDeviceDescriptor GetDefaultAudioDeviceDescriptor();
}
