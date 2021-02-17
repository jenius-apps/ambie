using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for signing in to a
    /// Microsoft account and retrieving a token.
    /// </summary>
    public interface IMsaAuthClient
    {
        /// <summary>
        /// Fires when sign in process completes.
        /// </summary>
        event EventHandler InteractiveSignInCompleted;

        /// <summary>
        /// Attempts to sign in silently and retrieve at token.
        /// Returns null if silent auth was unsuccessful.
        /// </summary>
        /// <returns>A token if sign in was successful, and null if not.</returns>
        Task<string> GetTokenSilentAsync();

        /// <summary>
        /// Attempts to sign in and retrieve at token. User will be prompted.
        /// Result will be communicated via <see cref="InteractiveSignInCompleted"/>.
        /// </summary>
        void RequestInteractiveSignIn();

        /// <summary>
        /// Attempts to retrieve the signed-in user's picture and returns
        /// a valid URI path. Return null of not signed in.
        /// </summary>
        /// <returns>URI path to user's profile picture. Null if not signed in.</returns>
        Task<string> GetPictureAsync();
    }
}