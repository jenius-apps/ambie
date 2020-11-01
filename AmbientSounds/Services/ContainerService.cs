using AmbientSounds.ViewModels;
using Autofac;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for configuring and building
    /// an Autofac container.
    /// </summary>
    public class ContainerService
    {
        /// <summary>
        /// Configures and builds an autofac container.
        /// </summary>
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainPageViewModel>();
            builder.RegisterType<PlayerViewModel>().SingleInstance();
            builder.RegisterType<MediaPlayerService>().SingleInstance();
            return builder.Build();
        }
    }
}
