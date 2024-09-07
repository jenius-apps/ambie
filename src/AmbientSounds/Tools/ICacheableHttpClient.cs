using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public interface ICacheableHttpClient
{
    Task<string> GetStringAsync(string? url);

    Task<Stream> GetStreamAsync(string? url);
}
