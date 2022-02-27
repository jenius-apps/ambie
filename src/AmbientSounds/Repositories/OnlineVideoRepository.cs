using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public class OnlineVideoRepository : IOnlineVideoRepository
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            await Task.Delay(1);
            return Array.Empty<Video>();
        }
    }
}
