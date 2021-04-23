using System.Diagnostics.CodeAnalysis;
using AmbientSounds.Models;
using Windows.ApplicationModel.Resources;

#nullable enable

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Static class used to localize strings.
    /// </summary>
    public static class LocalizationConverter
    {
        private static ResourceLoader? _loader;

        public static string ConvertPublishState(PublishState publishState)
        {
            InitializeLoader();
            return _loader.GetString("PublishState" + publishState.ToString());
        }

        /// <summary>
        /// Attempts to localize a sound's name.
        /// </summary>
        /// <param name="value">The <see cref="Models.Sound.Name"/> property.</param>
        /// <returns>A localized string of the sound if a localization exists.</returns>
        public static string ConvertSoundName(string value)
        {
            InitializeLoader();

            if (value is string soundName)
            {
                var translatedName = _loader.GetString("Sound" + soundName);
                return string.IsNullOrWhiteSpace(translatedName)
                    ? soundName
                    : translatedName;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Returns localized words for whether the player
        /// button can be paused or played.
        /// </summary>
        /// <param name="isPaused">Current state of the player.</param>
        public static string ConvertPlayerButtonState(bool isPaused)
        {
            InitializeLoader();
            return isPaused ? _loader.GetString("PlayerPlayText") : _loader.GetString("PlayerPauseText");
        }

        public static string SoundStatus(bool isCurrentlyPlaying)
        {
            InitializeLoader();
            if (isCurrentlyPlaying)
            {
                return _loader.GetString("Playing");
            }
            else
            {
                return _loader.GetString("Paused");
            }
        }

        /// <summary>
        /// Returns localized phrase for online sound object
        /// in a list view.
        /// </summary>
        /// <param name="name">Name of the sound.</param>
        /// <param name="canDownload">Whether or not the sound can be downloaded or is already downloaded.</param>
        /// <remarks>
        /// Generally used for AutomationProperties.Name.
        /// </remarks>
        public static string ConvertOnlineSoundListViewName(string name, bool canDownload)
        {
            if (_loader is null) _loader = ResourceLoader.GetForCurrentView();
            var result = name + ". ";
            result += canDownload 
                ? _loader.GetString("CanDownload") 
                : _loader.GetString("AlreadyDownloaded");

            return result;
        }

        [MemberNotNull(nameof(_loader))]
        private static void InitializeLoader()
        {
            if (_loader is null) _loader = ResourceLoader.GetForCurrentView();
        }
    }
}
