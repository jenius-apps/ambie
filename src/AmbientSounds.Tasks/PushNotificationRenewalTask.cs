using System;
using System.Threading.Tasks;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Settings.Uwp;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Background;

#nullable enable

namespace AmbientSounds.Tasks;

public sealed class PushNotificationRenewalTask : IBackgroundTask
{
    private IServiceProvider? _serviceProvider;

    public async void Run(IBackgroundTaskInstance taskInstance)
    {
        var d = taskInstance.GetDeferral();
        _serviceProvider = ConfigureServices();

        IUserSettings userSettings = _serviceProvider.GetRequiredService<IUserSettings>();

        if (userSettings.Get<bool>(UserSettingsConstants.Notifications) is false ||
            userSettings.Get<string>(UserSettingsConstants.LocalUserIdKey) is not { Length: > 0 } id)
        {
            return;
        }

        try
        {
#if DEBUG
            // Don't want to needlessly send messages to the notification service
            // when in debug mode.
            await Task.Delay(1);
#else
            await Services.GetRequiredService<Tools.IPushNotificationService>().RegisterAsync(
                id,
                Services.GetRequiredService<JeniusApps.Common.Tools.ISystemInfoProvider>().GetCulture(),
                default);
#endif
        }
        catch { }

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

    [Singleton(typeof(PushNotificationService), typeof(IPushNotificationService))]
    [Singleton(typeof(WindowsPushNotificationSource), typeof(IPushNotificationSource))]
    private static partial void ConfigureServices(IServiceCollection services);
}
