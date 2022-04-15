using System;
using System.Collections.Generic;

namespace AmbientSounds.Services
{
    public interface IFocusToastService
    {
        void ScheduleToasts(IReadOnlyList<FocusSession> orderedSessions, DateTime start);
    }
}