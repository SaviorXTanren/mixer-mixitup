using Mixer.Base.Model.Client;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mixer
{
    public abstract class MixerRequestWrapperBase
    {
        public async Task RunAsync(Task task) { await AsyncRunner.RunAsync(task); }

        public async Task<T> RunAsync<T>(Task<T> task, bool logNotFoundException = true) { return await AsyncRunner.RunAsync(task, logNotFoundException); }

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

        protected void WebSocketClient_OnPacketSentOccurred(object sender, WebSocketPacket e)
        {
            if (e is MethodPacket)
            {
                MethodPacket mPacket = (MethodPacket)e;
                Logger.Log(string.Format(Environment.NewLine + "WebSocket Method Sent: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, mPacket.method, mPacket.arguments, mPacket.parameters));
            }
            else if (e is ReplyPacket)
            {
                ReplyPacket rPacket = (ReplyPacket)e;
                Logger.Log(string.Format(Environment.NewLine + "WebSocket Reply Sent: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, rPacket.result, rPacket.error, rPacket.data));
            }
            else
            {
                Logger.Log(string.Format(Environment.NewLine + "WebSocket Packet Sent: {0} - {1}" + Environment.NewLine, e.id, e.type));
            }
        }

        protected void WebSocketClient_OnMethodOccurred(object sender, MethodPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Method Received: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.method, e.arguments, e.parameters));
        }

        protected void WebSocketClient_OnReplyOccurred(object sender, ReplyPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Reply Received: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.result, e.error, e.data));
        }

        protected void WebSocketClient_OnEventOccurred(object sender, EventPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Event Received: {0} - {1} - {2} - {3}" + Environment.NewLine, e.id, e.type, e.eventName, e.data));
        }
    }
}
