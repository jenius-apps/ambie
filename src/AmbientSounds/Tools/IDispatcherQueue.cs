using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Tools
{
    /// <summary>
    /// Wrapper interface around a dispatcher queue
    /// that the client can use to run code
    /// on the UI thread.
    /// </summary>
    public interface IDispatcherQueue
    {
        /// <summary>
        /// Schedules the given action to be run
        /// on the UI thread.
        /// </summary>
        void TryEnqueue(Action action);
    }
}
