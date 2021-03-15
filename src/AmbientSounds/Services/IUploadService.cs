using AmbientSounds.Models;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for uploading sounds to the catalogue.
    /// </summary>
    public interface IUploadService
    {
        /// <summary>
        /// Sound that was uploaded.
        /// </summary>
        event EventHandler<Sound>? SoundUploaded;

        /// <summary>
        /// Sound deleted from the user's upload list.
        /// Event args is the sound's Id.
        /// </summary>
        event EventHandler<string>? SoundDeleted;

        /// <summary>
        /// Uploads the given sound to the catalogue.
        /// </summary>
        /// <param name="s">The sound to upload.</param>
        Task UploadAsync(Sound s);

        /// <summary>
        /// Deletes the given sound from the catalogue.. 
        /// </summary>
        /// <param name="id">The Id of the sound to delete from the catalogue.</param>
        /// <returns>True if deletion was successful.</returns>
        Task<bool> DeleteAsync(string id);
    }
}