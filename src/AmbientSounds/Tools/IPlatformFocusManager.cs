using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Tools
{
    public interface IPlatformFocusManager
    {
        void DeactivateFocus();
        void TryStartFocus();
    }
}
