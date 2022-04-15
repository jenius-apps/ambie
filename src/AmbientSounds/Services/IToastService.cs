using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public interface IToastService
    {
        void ClearScheduledToasts();

        void ScheduleToast(
            DateTime scheduleDateTime,
            string title,
            string message);

        void SendToast(string title, string messages);
    }
}
