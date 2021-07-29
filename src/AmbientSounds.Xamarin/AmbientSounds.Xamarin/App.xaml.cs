using AmbientSounds.Services;
using AmbientSounds.Services.Xamarin;
using AmbientSounds.ViewModels;
using AmbientSounds.Xamarin.Native;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AmbientSounds.Xamarin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            RegisterDependencies();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void RegisterDependencies()
        {
            DependencyService.Register<ShellPageViewModel>();
            DependencyService.Register<IUserSettings, UserSettings>();
            DependencyService.Register<IScreensaverService, ScreensaverService>();
            DependencyService.Register<INavigator, Navigator>();
            DependencyService.Register<IDialogService, ModalService>();
            DependencyService.Register<IMixMediaPlayerService, MixMediaPlayer>();
            DependencyService.Register<ITelemetry, AppCenterTelemetry>();
            DependencyService.Register<IAppSettings, AppSettings>();
        }
    }
}
