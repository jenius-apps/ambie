using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class GuideService : IGuideService
{
    public async Task<IReadOnlyList<Guide>> GetGuidesAsync()
    {
        await Task.Delay(1);
        return Array.Empty<Guide>();
    }
}
