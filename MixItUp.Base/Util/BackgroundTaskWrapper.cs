using MixItUp.Base.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class BackgroundTaskWrapper
    {
        [Obsolete]
        public static async Task RunBackgroundTask(CancellationTokenSource tokenSource, Func<CancellationTokenSource, Task> backgroundTask)
        {
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    await backgroundTask(tokenSource);
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }
    }
}
