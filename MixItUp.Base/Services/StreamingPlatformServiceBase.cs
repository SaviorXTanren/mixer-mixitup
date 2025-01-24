using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class StreamingPlatformServiceBase
    {
        public abstract string Name { get; }

        protected async Task<Result> AttemptConnect(Func<Task<Result>> connect, int connectionAttempts = 5)
        {
            Result result = new Result();
            for (int i = 0; i < connectionAttempts; i++)
            {
                try
                {
                    result = await connect();
                    if (result.Success)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                await Task.Delay(1000);
            }
            return result;
        }

        protected async Task<IEnumerable<T>> RunAsync<T>(Task<IEnumerable<T>> task)
        {
            IEnumerable<T> result = null;
            try
            {
                result = await AsyncRunner.RunAsync(task);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            if (result == null)
            {
                result = ReflectionHelper.CreateInstanceOf<List<T>>();
            }
            return result;
        }
    }
}
