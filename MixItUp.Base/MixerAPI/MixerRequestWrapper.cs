using Mixer.Base.Model.Client;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public abstract class MixerRequestWrapperBase
    {
        public async Task RunAsync(Task task) { await AsyncRunner.RunAsync(task); }

        public async Task<T> RunAsync<T>(Task<T> task) { return await AsyncRunner.RunAsync(task); }

        public async Task RunAsync(Func<Task> task) { await AsyncRunner.RunAsync(task); }

        public async Task<T> RunAsync<T>(Func<Task<T>> task) { return await AsyncRunner.RunAsync(task); }

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

        protected void WebSocketClient_OnMethodOccurred(object sender, MethodPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Method: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.method, e.arguments, e.parameters));
        }

        protected void WebSocketClient_OnReplyOccurred(object sender, ReplyPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Reply: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.result, e.error, e.data));
        }

        protected void WebSocketClient_OnEventOccurred(object sender, EventPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Event: {0} - {1} - {2} - {3}" + Environment.NewLine, e.id, e.type, e.eventName, e.data));
        }
    }
}
