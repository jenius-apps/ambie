using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public class AzureServiceBusPushNotificationStorage : IPushNotificationStorage
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public AzureServiceBusPushNotificationStorage(string connectionString, string queueName)
    {
        // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
        // If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open.
        // Ref: https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues?tabs=connection-string#add-code-to-send-messages-to-the-queue
        var clientOptions = new ServiceBusClientOptions()
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        _client = new ServiceBusClient(connectionString, clientOptions);
        _sender = _client.CreateSender(queueName);
    }

    /// <inheritdoc/>
    public async Task DeleteDeviceRegistrationAsync(string deviceId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        DeviceRegistrationData data = new()
        {
            ActionRequested = "unregister",
            DeviceId = deviceId,
            PrimaryLanguageCode = string.Empty,
            Uri = string.Empty
        };

        var message = JsonSerializer.Serialize(data);
        await _sender.SendMessageAsync(new ServiceBusMessage(message), ct);
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterDeviceAsync(DeviceRegistrationData data, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var message = JsonSerializer.Serialize(data);
        await _sender.SendMessageAsync(new ServiceBusMessage(message), ct);
        return true;
    }
}
