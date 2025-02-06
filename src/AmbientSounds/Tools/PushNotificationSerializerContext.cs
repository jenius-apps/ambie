using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmbientSounds.Tools;

[JsonSerializable(typeof(DeviceRegistrationData))]
public sealed partial class PushNotificationSerializerContext : JsonSerializerContext
{
    /// <summary>
    /// The lazily initialized backing field for the context to be used for case insensitive serialization (<see cref="CaseInsensitive"/>).
    /// </summary>
    private static PushNotificationSerializerContext? _caseInsensitive;

    /// <summary>
    /// A case insensitive variant of <see cref="Default"/>.
    /// </summary>
    public static PushNotificationSerializerContext CaseInsensitive => _caseInsensitive ??= new PushNotificationSerializerContext(new JsonSerializerOptions(s_defaultOptions) { PropertyNameCaseInsensitive = true });
}