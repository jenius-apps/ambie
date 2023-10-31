using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Service responsible for enabling and disabling quick resume.
/// </summary>
public interface IQuickResumeService
{
    /// <summary>
    /// Attempts to enable the quick resume feature.
    /// </summary>
    /// <returns>True if successful. False, otherwise.</returns>
    Task<bool> TryEnableAsync();

    /// <summary>
    /// Disables the quick resume feature.
    /// </summary>
    void Disable();
}
