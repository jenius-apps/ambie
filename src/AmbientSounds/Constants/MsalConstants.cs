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

        /// <summary>
        /// Scopres required by Ambie that are related to the
        /// Ambie Catalogue.
        /// </summary>
        public static readonly string[] CatalogueScopes = new string[]
        {
            "api://46dc6fa7-8bf8-40ac-8de9-ad73e1fce9fe/Catalogue.Publish"
        };
    }
}
