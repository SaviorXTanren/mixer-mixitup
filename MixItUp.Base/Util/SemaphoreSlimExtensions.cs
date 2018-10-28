using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class SemaphoreSlimExtensions
    {
        public static async Task WaitAndRelease(this SemaphoreSlim semaphore, Func<Task> function)
        {
            try
            {
                await semaphore.WaitAsync();

                await function();
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally { semaphore.Release(); }
        }

        public static async Task<T> WaitAndRelease<T>(this SemaphoreSlim semaphore, Func<Task<T>> function)
        {
            try
            {
                await semaphore.WaitAsync();

                return await function();
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally { semaphore.Release(); }
            return default(T);
        }
    }
}
