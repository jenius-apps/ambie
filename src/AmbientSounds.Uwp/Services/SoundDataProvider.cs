using AmbientSounds.Converters;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// A provider of sound data.
    /// </summary>
    public sealed class SoundDataProvider : ISoundDataProvider
    {
        private const string DataFileName = "Data.json";
        private const string LocalDataFileName = "localData.json";
        private List<Sound>? _localSoundCache; // cache of non-packaged sounds.
        private List<Sound>? _packagedSoundCache;
        private Random? _random;
    }
}
