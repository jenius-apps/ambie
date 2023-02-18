using AmbientSounds.Services;
using AmbientSounds.Services.Xamarin;
using AmbientSounds.Tools;
using AmbientSounds.ViewModels;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Forms;

#nullable enable

namespace AmbientSounds.Xamarin
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        public App()
        {
            InitializeComponent();
            _serviceProvider = RegisterDependencies();
            MainPage = new Views.TabbedShellPage();
        }

        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider? serviceProvider = ((App)Current)._serviceProvider;

                if (serviceProvider is null)
                {
                    ThrowHelper.ThrowInvalidOperationException("The service provider is not initialized");
                }

                return serviceProvider;
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private IServiceProvider RegisterDependencies()
        {
            var provider = new ServiceCollection()
                .AddSingleton<ShellPageViewModel>()
                .AddSingleton<IUserSettings, UserSettings>()
                .AddSingleton<IScreensaverService, ScreensaverService>()
                .AddSingleton<INavigator, Navigator>()
                .AddSingleton<IDialogService, ModalService>()
                .AddSingleton<IMixMediaPlayerService, MixMediaPlayerService>()
                .AddSingleton<ITelemetry, AppCenterTelemetry>()
                .AddSingleton<IAppSettings, AppSettings>()
                // Must be transient because this is basically
                // a timer factory.
                .AddTransient<ITimerService, TimerService>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
