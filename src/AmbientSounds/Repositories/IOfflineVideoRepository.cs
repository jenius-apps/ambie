using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOfflineVideoRepository
    {
        /// <summary>
        /// Retrives a list of video metadata from
        /// an offline source.
        /// </summary>
        Task<IReadOnlyList<Video>> GetVideosAsync();
    }
}
