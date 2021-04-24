using AmbientSounds.Constants;
using AmbientSounds.Strings;

#nullable enable

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Class for retrieving the 
    /// the error message based on the error id.
    /// </summary>
    public static class ErrorLoader
    {
        public static string GetMessage(this string errorId, string? customMessage = null)
        {
            var result = "";
            switch (errorId)
            {
                case ErrorConstants.BigFileId:
                    result = Resources.ErrorBigFile.FormatHelper(ErrorConstants.SizeLimit.ToString());
                    break;
                case ErrorConstants.UploadLimitId:
                    result = Resources.ErrorUploadCount.FormatHelper(ErrorConstants.UploadLimit.ToString());
                    break;
                case ErrorConstants.CustomId:
                    result = customMessage ?? "";
                    break;
                default:
                    break;
            }

            return result;
        }

        private static string FormatHelper(this string template, string arg)
            => string.Format(template, arg);
    }
}
