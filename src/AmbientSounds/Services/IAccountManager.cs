using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for a central hub that handles accounts.
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Raised when sign in changes. If user is signed in, 
        /// the result is true.
        /// </summary>
        event EventHandler<bool>? SignInUpdated;

        /// <summary>
        /// 'Signed in' is defined as being able to retrieve
        /// a valid token silently. So this method will attempt to retrieve
        /// a token silently. If successfull, it will return true.
        /// </summary>
        /// <returns>True if we are able to retrieve a token silently.</returns>
        Task<bool> IsSignedInAsync();

        /// <summary>
        /// Signs the user in. This will launch several prompts to the user
        /// to perform the sign in process. Result will be communicated
        /// by <see cref="SignInUpdated"/>.
        /// </summary>
        void RequestSignIn();

        /// <summary>
        /// Retrieves the path to the user's picture.
        /// </summary>
        /// <returns>Path to user's picture.</returns>
        Task<string> GetPictureAsync();
    }
}
