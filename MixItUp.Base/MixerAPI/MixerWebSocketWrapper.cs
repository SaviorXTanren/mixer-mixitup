using Mixer.Base.Model.Client;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public abstract class MixerWebSocketWrapper : AsyncRequestServiceBase
    {
        public bool ShouldRetry { get; set; } = true;

        protected CancellationTokenSource backgroundThreadCancellationTokenSource;

        public async Task<bool> AttemptConnect(int connectionAttempts = 5)
        {
            for (int i = 0; i < connectionAttempts; i++)
            {
                if (await ConnectInternal())
                {
                    return true;
                }

                if (!ShouldRetry)
                {
                    return false;
                }

                await Task.Delay(1000);
            }
            return false;
        }

        protected abstract Task<bool> ConnectInternal();

        protected void WebSocketClient_OnPacketSentOccurred(object sender, WebSocketPacket e)
        {
            if (e is MethodPacket)
            {
                MethodPacket mPacket = (MethodPacket)e;
                Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Method Sent: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, mPacket.method, mPacket.arguments, mPacket.parameters));
            }
            else if (e is ReplyPacket)
            {
                ReplyPacket rPacket = (ReplyPacket)e;
                Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Reply Sent: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, rPacket.result, rPacket.error, rPacket.data));
            }
            else
            {
                Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Packet Sent: {0} - {1}" + Environment.NewLine, e.id, e.type));
            }
        }

        protected void WebSocketClient_OnMethodOccurred(object sender, MethodPacket e)
        {
            Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Method Received: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.method, e.arguments, e.parameters));
        }

        protected void WebSocketClient_OnReplyOccurred(object sender, ReplyPacket e)
        {
            Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Reply Received: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.result, e.error, e.data));
        }

        protected void WebSocketClient_OnEventOccurred(object sender, EventPacket e)
        {
            Logger.Log(LogLevel.Debug, string.Format(Environment.NewLine + "WebSocket Event Received: {0} - {1} - {2} - {3}" + Environment.NewLine, e.id, e.type, e.eventName, e.data));
        }
    }
}
