using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// A model for a MSA user retrieved from Microsoft Graph.
/// </summary>
public sealed record MsalUser(
    [property: JsonPropertyName("givenName"), JsonRequired] string FirstName,
    [property: JsonPropertyName("userPrincipalName"), JsonRequired] string Email);
