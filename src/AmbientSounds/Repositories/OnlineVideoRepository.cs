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
        public Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            throw new NotImplementedException();
        }
    }
}
