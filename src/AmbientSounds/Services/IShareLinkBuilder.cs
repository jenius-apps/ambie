namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for buiding a share sounds link.
    /// </summary>
    public interface IShareLinkBuilder
    {
        /// <summary>
        /// Generates a share sounds link based
        /// on the currently playing sounds.
        /// </summary>
        /// <returns>String representing an ambie URL. E.g. ambie://play?sounds=foobar </returns>
        string GetLink();
    }
}