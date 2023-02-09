namespace AmbientSounds.Models;

public class DisplayInformation
{
    /// <summary>
    /// The language code for this display information.
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// The localized name to be displayed.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The localized description to be displayed.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}