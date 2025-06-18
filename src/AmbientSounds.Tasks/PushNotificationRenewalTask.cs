using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Extensions.DependencyInjection;
using JeniusApps.Common.PushNotifications;
using JeniusApps.Common.PushNotifications.Uwp;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Settings.Uwp;
using JeniusApps.Common.Tools;
using JeniusApps.Common.Tools.Uwp;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.ApplicationModel.Background;

#nullable enable

namespace AmbientSounds.Tasks;

public sealed partial class PushNotificationRenewalTask : IBackgroundTask
{
    private IServiceProvider? _serviceProvider;

    public async void Run(IBackgroundTaskInstance taskInstance)
    {
        var d = taskInstance.GetDeferral();
        _serviceProvider = ConfigureServices();

        _ = await _serviceProvider.GetRequiredService<IPushNotificationRegistrationService>().TryRegisterPushNotificationsAsync();

        d.Complete();
    }

    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection collection = new();
        ConfigureServices(collection);

        collection.AddSingleton<IUserSettings, LocalSettings>(s => new LocalSettings(UserSettingsConstants.Defaults));
        collection.AddSingleton<IPushNotificationStorage, AzureServiceBusPushNotificationStorage>(s =>
        {
            var connectionString = s.GetRequiredService<IAppSettings>().NotificationHubConnectionString;
            var queueName = s.GetRequiredService<IAppSettings>().NotificationHubName;
            return new AzureServiceBusPushNotificationStorage(connectionString, queueName);
        });

        IServiceProvider provider = collection.BuildServiceProvider();
        return provider;

    }

    [Singleton(typeof(PushNotificationRegistrationService), typeof(IPushNotificationRegistrationService))]
    [Singleton(typeof(SystemInfoProvider), typeof(ISystemInfoProvider))]
    [Singleton(typeof(PushNotificationService), typeof(IPushNotificationService))]
    [Singleton(typeof(WindowsPushNotificationSource), typeof(IPushNotificationSource))]
    private static partial void ConfigureServices(IServiceCollection services);
}
