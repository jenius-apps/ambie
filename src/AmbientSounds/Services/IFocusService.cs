using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public interface IFocusService
    {
        TimeSpan GetTotalTime(int focusLength, int restLength, int repetitions);
    }
}
