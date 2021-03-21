using AmbientSounds.Constants;
using AmbientSounds.Strings;

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Class for retrieving the 
    /// the error message based on the error id.
    /// </summary>
    public static class ErrorLoader
    {
        public static string GetMessage(this string errorId)
        {
            if (errorId == ErrorConstants.BigFileId)
            {
                return string.Format(Resources.ErrorBigFile, ErrorConstants.SizeLimit);
            }

            return "";
        }
    }
}
