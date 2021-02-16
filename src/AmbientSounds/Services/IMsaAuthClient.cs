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
        /// Attempts to sign in silently and retrieve at token.
        /// Returns null if silent auth was unsuccessful.
        /// </summary>
        /// <returns>A token if sign in was successful, and null if not.</returns>
        Task<string> GetTokenSilentAsync();
    }
}