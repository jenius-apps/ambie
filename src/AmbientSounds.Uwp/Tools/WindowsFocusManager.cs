using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Tools.Uwp
{
    public class WindowsFocusManager : IPlatformFocusManager
    {
        /**
         * Turns out, these APIs are limited access features, so I need
         * an access token to unlock them. Without an access token, these
         * will throw Access Denied exceptions.
         */

        public void TryStartFocus()
        {
            var session = Windows.UI.Shell.FocusSessionManager.GetDefault().TryStartFocusSession();
        }

        public void DeactivateFocus()
        {
            Windows.UI.Shell.FocusSessionManager.GetDefault().DeactivateFocus();
        }
    }
}
