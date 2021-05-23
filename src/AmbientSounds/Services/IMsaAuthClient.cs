using AmbientSounds.Models;
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
        event EventHandler? InteractiveSignInCompleted;

        /// <summary>
        /// Attempts to sign in silently and retrieve a token
        /// for the given scopes.
        /// Returns null if silent auth was unsuccessful.
        /// </summary>
        /// <returns>A token if sign in was successful, and null if not.</returns>
        Task<string?> GetTokenSilentAsync(string[] scopes);

        /// <summary>
        /// Attempts to sign in and retrieve at token. User will be prompted.
        /// Result will be communicated via <see cref="InteractiveSignInCompleted"/>.
        /// </summary>
        Task RequestInteractiveSignIn(
            string[] scopes,
            string[]? extraScopes = null);

        /// <summary>
        /// Attempts to retrieve the signed-in user's data and returns
        /// it.
        /// </summary>
        /// <returns>If sign in and permission is successful, returns a populated 
        /// Person object. If unsuccessful, returns an default person object.</returns>
        Task<Person> GetPersonDataAsync();

        /// <summary>
        /// Signs out the user.
        /// </summary>
        Task SignOutAsync();
    }
}