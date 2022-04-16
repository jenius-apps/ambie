using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public class FocusToastService : IFocusToastService
    {
        private readonly IToastService _toastService;

        public FocusToastService(IToastService toastService)
        {
            Guard.IsNotNull(toastService, nameof(toastService));

            _toastService = toastService;
        }

        public void ClearToasts()
        {
            _toastService.ClearScheduledToasts();
        }

        public void ScheduleToasts(IReadOnlyList<FocusSession> orderedSessions, DateTime start, bool showStartToast)
        {
            _toastService.ClearScheduledToasts();

            if (showStartToast)
            {
                _toastService.SendToast("Starting focus session", "We'll let you know when it's time to rest!");
            }

            var current = start;
            foreach (var session in orderedSessions)
            {
                current = current.AddTicks(session.Remaining.Ticks);

                // TODO need to schedule toasts using a group id so that they can be cancelled without cancelling others.
                if (session.QueuePosition == session.QueueSize - 1)
                {
                    _toastService.ScheduleToast(current, "Focus session complete", "Great job focusing!");
                }
                else
                {
                    if (session.SessionType == SessionType.Focus)
                    {
                        _toastService.ScheduleToast(current, "Rest", "Time to rest");
                    }
                    else if (session.SessionType == SessionType.Rest)
                    {
                        _toastService.ScheduleToast(current, "Focus", "Time to focus");
                    }
                }
            }
        }
    }
}
