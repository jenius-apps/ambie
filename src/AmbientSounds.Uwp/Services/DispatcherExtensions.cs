using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace AmbientSounds.Services.Uwp
{
    // Ref: https://docs.microsoft.com/en-us/uwp/api/windows.ui.core.coredispatcher.runasync?view=winrt-19041#await-a-ui-task-sent-from-a-background-thread
    // Source: https://github.com/Microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md

    public static class DispatcherExtensions
    {
        public static async Task<T> RunTaskAsync<T>(this CoreDispatcher dispatcher,
            Func<Task<T>> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static async Task RunTaskAsync(this CoreDispatcher dispatcher,
            Func<Task> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
            await RunTaskAsync(dispatcher, async () => { await func(); return false; }, priority);
    }
}
