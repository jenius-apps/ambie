using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AmbientSounds.Services.Uwp
{
    public class ToastService : IToastService
    {
        public void ClearScheduledToasts()
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

        public void ScheduleToast(
            DateTime scheduleDateTime,
            string title,
            string message,
            bool silent = false)
        {
            if (scheduleDateTime <= DateTime.Now)
            {
                return;
            }

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Alarm)
                .AddAudio(new Uri("ms-appx:///Assets/SoundEffects/bell.wav"), silent: silent)
                .AddButton(new ToastButtonDismiss())
                .AddText(title)
                .AddText(message)
                .Schedule(scheduleDateTime);
        }

        public void SendToast(string title, string message)
        {
            new ToastContentBuilder()
                .AddButton(new ToastButtonDismiss())
                .AddText(title)
                .AddText(message)
                .Show();
        }
    }
}
