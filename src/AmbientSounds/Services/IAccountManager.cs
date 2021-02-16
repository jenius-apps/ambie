using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IAccountManager
    {
        /// <summary>
        /// 'Signed in' is defined as being able to retrieve
        /// a valid token silently. So this method will attempt to retrieve
        /// a token silently. If successfull, it will return true.
        /// </summary>
        /// <returns>True if we are able to retrieve a token silently.</returns>
        Task<bool> IsSignedInAsync();
    }
}
