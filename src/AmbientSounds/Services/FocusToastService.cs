using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using AmbientSounds.Constants;

namespace AmbientSounds.Services
{
    public class FocusToastService : IFocusToastService
    {
        private readonly IToastService _toastService;
        private readonly ILocalizer _localizer;

        public FocusToastService(
            IToastService toastService,
            ILocalizer localizer)
        {
            Guard.IsNotNull(toastService, nameof(toastService));
            Guard.IsNotNull(localizer, nameof(localizer));

            _toastService = toastService;
            _localizer = localizer;
        }

        public void ClearToasts()
        {
            _toastService.ClearScheduledToasts();
        }

        public void SendCompletionToast()
        {
            _toastService.SendToast(
                _localizer.GetString("FocusSessionCompleteTitle"),
                _localizer.GetString("FocusSessionCompleteMessage"),
                LaunchConstants.FocusCompleteArgument);
        }

        public void ScheduleToasts(IReadOnlyList<FocusSession> orderedSessions, DateTime start)
        {
            _toastService.ClearScheduledToasts();

            var current = start;
            foreach (var session in orderedSessions)        
            {
                current = current.AddTicks(session.Remaining.Ticks);

                if (session.QueuePosition == session.QueueSize - 1)
                {
                    // last session, so no need to schedule a toast.
                    continue;
                }
                else
                {
                    if (session.SessionType == Models.SessionType.Focus)
                    {
                        _toastService.ScheduleToast(
                            current,
                            _localizer.GetString("RestText"),
                            _localizer.GetString("FocusSessionRestMessage"),
                            silent: false,
                            launchArg: LaunchConstants.FocusSegmentArgument);
                    }
                    else if (session.SessionType == Models.SessionType.Rest)
                    {
                        _toastService.ScheduleToast(
                            current,
                            _localizer.GetString("FocusText"),
                            _localizer.GetString("FocusSessionFocusMessage"),
                            silent: true,
                            launchArg: LaunchConstants.FocusSegmentArgument);
                    }
                }
            }
        }
    }
}
