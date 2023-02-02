﻿using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IUpdateService
    {
        Task<IReadOnlyList<Sound>> CheckForUpdatesAsync();
    }
}