namespace AmbientSounds.Services
{
    /// <summary>
    /// Allows programmatic page navigation.
    /// </summary>
    public interface INavigator
    {
        /// <summary>
        /// The frame that can navigate. This must be set before
        /// any method is called.
        /// </summary>
        object Frame { get; set; }

        /// <summary>
        /// Navigates to the screensaver.
        /// </summary>
        void ToScreensaver();

        /// <summary>
        /// Navigates to the compact page.
        /// </summary>
        void ToCompact();

        /// <summary>
        /// Navigates to the upload page.
        /// </summary>
        void ToUploadPage();
    }
}