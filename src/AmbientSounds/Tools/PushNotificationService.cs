using CommunityToolkit.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushNotificationSource _source;
    private readonly IPushNotificationStorage _storage;

    public PushNotificationService(
        IPushNotificationSource source,
        IPushNotificationStorage storage)
    {
        _source = source;
        _storage = storage;
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterAsync(string deviceId, string primaryLanguageCode, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(deviceId))
        {
            ThrowHelper.ThrowArgumentException(nameof(deviceId));
        }

        if (string.IsNullOrEmpty(primaryLanguageCode))
        {
            ThrowHelper.ThrowArgumentException(nameof(primaryLanguageCode));
        }

        string? uri = await _source.GetNotificationUriAsync(ct);
        if (uri is not { Length: > 0 })
        {
            return false;
        }

        DeviceRegistrationData data = new()
        {
            DeviceId = deviceId,
            PrimaryLanguageCode = primaryLanguageCode,
            Uri = uri
        };

        return await _storage.RegisterDeviceAsync(data, ct);
    }

    /// <inheritdoc/>
    public async Task UnregisterAsync(string deviceId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await _source.UnregisterAsync(ct);
        await _storage.DeleteDeviceRegistrationAsync(deviceId, ct);
    }
}
