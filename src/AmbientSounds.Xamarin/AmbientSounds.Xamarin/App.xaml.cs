using AmbientSounds.Services;
using AmbientSounds.Services.Xamarin;
using AmbientSounds.ViewModels;
using Xamarin.Forms;

namespace AmbientSounds.Xamarin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            RegisterDependencies(); // TODO replace this with the dependency injection extension by MS
            MainPage = new Views.TabbedShellPage();
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
