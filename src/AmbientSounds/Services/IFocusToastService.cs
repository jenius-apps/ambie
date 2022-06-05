using System;
using System.Collections.Generic;

namespace AmbientSounds.Services
{
    public interface IFocusToastService
    {
        void ClearToasts();

        void SendCompletionToast();

        void ScheduleToasts(IReadOnlyList<FocusSession> orderedSessions, DateTime start);
    }
}