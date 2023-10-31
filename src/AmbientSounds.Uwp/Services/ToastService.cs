using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AmbientSounds.Services.Uwp
{
    public class ToastService : IToastService
    {
        private const string BellUriString = "ms-appx:///Assets/SoundEffects/bell.wav";
        private readonly Uri _bellUri;
        private readonly Lazy<IToastButton> _dismissButton;

        public ToastService()
        {
            _bellUri = new Uri(BellUriString);
            _dismissButton = new Lazy<IToastButton>(() => new ToastButtonDismiss());
        }

        public void ClearScheduledToasts()
        {
            try
            {
                ToastNotifierCompat notifier = ToastNotificationManagerCompat.CreateToastNotifier();
                var scheduled = notifier.GetScheduledToastNotifications();

                if (scheduled != null)
                {
                    foreach (var toRemove in scheduled)
                    {
                        notifier.RemoveFromSchedule(toRemove);
                    }
                }
            }
            catch
            {
                // Crash telemetry suggests that sometimes, the 
                // "notification platform" is unavailable, leading to a crash somehere here.
                // We added the try-catch to try to mitigate the crash.
            }
        }

        public void ScheduleToast(
            DateTime scheduleDateTime,
            string title,
            string message,
            bool silent = false,
            string launchArg = "scheduledToast")
        {
            if (scheduleDateTime <= DateTime.Now)
            {
                return;
            }

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddAudio(_bellUri, silent: silent)
                .AddButton(_dismissButton.Value)
                .AddText(title)
                .AddText(message)
                .AddArgument(launchArg)
                .Schedule(scheduleDateTime, toast =>
                {
                    toast.ExpirationTime = new DateTimeOffset(scheduleDateTime).AddMinutes(1);
                });
        }

        public void SendToast(string title, string message, string launchArg = "singleToast")
        {
            new ToastContentBuilder()
                .AddButton(_dismissButton.Value)
                .AddText(title)
                .AddText(message)
                .AddArgument(launchArg)
                .Show(toast =>
                {
                    toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(1);
                });
        }
    }
}
