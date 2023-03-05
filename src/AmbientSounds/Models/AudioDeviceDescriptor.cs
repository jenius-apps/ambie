using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Models;

public class AudioDeviceDescriptor
{
    /// <summary>
    /// A string representing the identity of the device.
    /// See also docs of <seealso href="https://learn.microsoft.com/en-us/uwp/api/windows.devices.enumeration.deviceinformation.id">DeviceInformation.Id</seealso>
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// The name of the device. This name is in the best available language for the app.
    /// </summary>
    public string DeviceName { get; init; } = string.Empty;

}
