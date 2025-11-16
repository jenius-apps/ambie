namespace AmbientSounds.Models;

/// <summary>
/// Defines the state that the user is in
/// with regards to if they're a premium user or not.
/// </summary>
public enum PremiumState
{
    /// <summary>
    /// Default state. Used when we have yet
    /// to evaluate the user's premium state.
    /// </summary>
    Unknown,

    /// <summary>
    /// User has been evaluated to be a free user.
    /// </summary>
    Free,

    /// <summary>
    /// User has been evaluated to be a premium user.
    /// </summary>
    Premium
}
