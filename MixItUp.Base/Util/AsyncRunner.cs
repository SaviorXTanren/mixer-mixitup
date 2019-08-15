using Mixer.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class AsyncRunner
    {
        public static async Task RunAsync(Task task, bool logNotFoundException = true)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (!logNotFoundException && ex is HttpRestRequestException)
                {
                    HttpRestRequestException restEx = (HttpRestRequestException)ex;
                    if (restEx.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }
                }
                Logger.Log(ex, includeStackTrace: true);
            }
        }

        public static async Task<T> RunAsync<T>(Task<T> task, bool logNotFoundException = true)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                if (!logNotFoundException && ex is HttpRestRequestException)
                {
                    HttpRestRequestException restEx = (HttpRestRequestException)ex;
                    if (restEx.StatusCode == HttpStatusCode.NotFound)
                    {
                        return default(T);
                    }
                }
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
    }
}
