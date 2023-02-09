using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Returns true is the current device is a Desktop.
        /// </summary>
        bool IsDesktop();

        /// <summary>
        /// Returns true is the current
        /// device is Xbox or other device
        /// optimized for a 10-foot viewing
        /// distance.
        /// </summary>
        bool IsTenFoot();

        /// <summary>
        /// Returns true if the current
        /// session is the first time this app
        /// was run since being installed.
        /// </summary>
        bool IsFirstRun();

        /// <summary>
        /// Returns true if the app is currently in compact mode.
        /// </summary>
        bool IsCompact();
        
        /// <summary>
        /// Returns true if the system is capable of using
        /// the built-in fluent system icons.
        /// </summary>
        bool CanUseFluentSystemIcons();

        /// <summary>
        /// Returns the date time when the app was first used.
        /// </summary>
        /// <returns>DateTime when the app was first used.</returns>
        DateTime FirstUseDate();
    }
}
