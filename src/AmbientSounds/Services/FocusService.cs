using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public class FocusService : IFocusService
    {
        public TimeSpan GetTotalTime(int focusLength, int restLength, int repetitions)
        {
            if (focusLength < 0 ||
                restLength < 0 ||
                repetitions < 0)
            {
                return TimeSpan.Zero;
            }

            repetitions += 1;
            return TimeSpan.FromMinutes((focusLength + restLength) * repetitions);
        }
    }
}
