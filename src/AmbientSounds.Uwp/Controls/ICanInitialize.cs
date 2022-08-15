using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Controls
{
    public interface ICanInitialize
    {
        Task InitializeAsync();

        void Uninitialize();
    }
}
