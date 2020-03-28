using StreamingClient.Base.Util;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class AsyncRunner
    {
        public static async Task RunAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
        }

        public static async Task<T> RunAsync<T>(Task<T> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
            return default(T);
        }

        public static async Task RunAsync(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
        }

        public static async Task<T> RunAsync<T>(Func<Task<T>> task)
        {
            try
            {
                return await task();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
            return default(T);
        }

        public static void RunAsyncInBackground(Func<Task> task)
        {
            Task.Run(async () =>
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, includeStackTrace: true);
                }
            });
        }

        public static async Task RunSyncAsAsync(Action action)
        {
            await Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        public static async Task<T> RunSyncAsAsync<T>(Func<T> function)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                return default(T);
            });
        }

        public static void RunAsyncAsSync(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
        }

        public static void RunBackgroundTask(CancellationToken token, Func<CancellationToken, Task> backgroundTask)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    await backgroundTask(token);
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static void RunBackgroundTask(CancellationToken token, int delayInMilliseconds, Func<CancellationToken, Task> backgroundTask)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await backgroundTask(token);

                        if (delayInMilliseconds > 0 && !token.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMilliseconds, token);
                        }
                    }
                    catch (ThreadAbortException) { return; }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}