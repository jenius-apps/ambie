using System;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for deciphering a uri and performing
    /// its actions.
    /// </summary>
    public interface ILinkProcessor
    {
        /// <summary>
        /// Procceses the given uri.
        /// </summary>
        /// <param name="uri"></param>
        void Process(Uri uri);
    }
}