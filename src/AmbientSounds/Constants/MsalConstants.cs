namespace AmbientSounds.Constants
{
    /// <summary>
    /// Class for constants related to MSAL.
    /// </summary>
    public class MsalConstants
    {
        /// <summary>
        /// Scopes required by Ambie that are related to MS Graph.
        /// </summary>
        public static readonly string[] GraphScopes = new string[]
        {
            "https://graph.microsoft.com/User.Read",
            "https://graph.microsoft.com/Files.ReadWrite.AppFolder"
        };
    }
}
