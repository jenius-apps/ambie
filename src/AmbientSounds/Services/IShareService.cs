using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public interface IShareService
{
    event EventHandler<IReadOnlyList<string>>? ShareRequested;

    IReadOnlyList<string>? RecentShare { get; }

    Task ProcessShareRequestAsync(string shareId);

    Task<string> GetShareUrlAsync(IReadOnlyList<string> soundIds);
}