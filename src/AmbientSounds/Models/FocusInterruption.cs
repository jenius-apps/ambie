using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Models
{
    public class FocusInterruption
    {
        public long UtcTime { get; set; }

        public double Minutes { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
