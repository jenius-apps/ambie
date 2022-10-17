using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public sealed class ShellPageNavigationArgs
    {
        /// <summary>
        /// If true, describes that Ambie was activated
        /// as an Xbox Game Bar widget.
        /// </summary>
        public bool IsGameBarWidget { get; set; }
    }
}
