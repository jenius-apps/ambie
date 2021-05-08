namespace AmbientSounds.Constants
{
    /// <summary>
    /// Class for storing constants
    /// related to errors.
    /// </summary>
    public class ErrorConstants
    {
        // Threshold values
        public const double SizeLimit = 25;
        public const double UploadLimit = double.MaxValue;

        // IDs
        public const string BigFileId = "big_file";
        public const string UploadLimitId = "upload_count";
        public const string CustomId = "custom_message";
    }
}
