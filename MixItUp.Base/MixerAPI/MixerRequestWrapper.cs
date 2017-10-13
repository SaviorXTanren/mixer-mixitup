using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public abstract class MixerRequestWrapperBase
    {
        public async Task RunAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<T> RunAsync<T>(Task<T> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return default(T);
        }

        public async Task<IEnumerable<T>> RunAsync<T>(Task<IEnumerable<T>> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return ReflectionHelper.CreateInstanceOf<List<T>>();
        }

        public async Task<IDictionary<K,V>> RunAsync<K,V>(Task<IDictionary<K, V>> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return ReflectionHelper.CreateInstanceOf<Dictionary<K,V>>();
        }
    }
}
