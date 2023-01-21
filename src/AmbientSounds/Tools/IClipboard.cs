using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public interface IClipboard
{
    Task<bool> CopyToClipboardAsync(string text);
}
