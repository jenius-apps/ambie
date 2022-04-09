using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public interface IFocusService
    {
        event EventHandler<FocusSession>? TimeUpdated;

        TimeSpan GetTotalTime(int focusLength, int restLength, int repetitions);
        void PauseTimer();
        void ResumeTimer();
        void StartTimer(int focusLength, int restLength, int repetitions);
        void StopTimer();
    }
}
