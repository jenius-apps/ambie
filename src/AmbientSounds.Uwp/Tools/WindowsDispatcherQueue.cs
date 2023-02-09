using System;
using Windows.System;

namespace AmbientSounds.Tools.Uwp
{
    /// <summary>
    /// Wrapper class around DispatcherQueue.
    /// </summary>
    public class WindowsDispatcherQueue : IDispatcherQueue
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public WindowsDispatcherQueue()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        /// <inheritdoc/>
        public void TryEnqueue(Action action)
        {
            _dispatcherQueue.TryEnqueue(() => action());
        }
    }
}
