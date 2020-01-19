using Mixer.Base.Model.Client;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mixer
{
    public abstract class MixerWebSocketServiceBase : MixerRequestWrapperBase
    {
        public async Task<bool> AttemptConnect(Func<Task<bool>> connect, int connectionAttempts = 5)
        {
            for (int i = 0; i < connectionAttempts; i++)
            {
                try
                {
                    if (await connect())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                await Task.Delay(1000);
            }
            return false;
        }

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
