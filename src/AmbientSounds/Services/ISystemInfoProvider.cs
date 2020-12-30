namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for getting system information
    /// for the current session.
    /// </summary>
    public interface ISystemInfoProvider
    {
        /// <summary>
        /// Retrieves the culture name
        /// in en-US format.
        /// </summary>
        string GetCulture();
    }
}
