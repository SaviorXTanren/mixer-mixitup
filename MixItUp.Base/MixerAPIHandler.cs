using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class MixerAPIHandler
    {
        public static MixerConnection MixerConnection { get; private set; }
        public static ChatClient ChatClient { get; private set; }

        public static async Task<bool> InitializeMixerClient(string clientID, IEnumerable<ClientScopeEnum> scopes, Action<string> codeCallback)
        {
            MixerAPIHandler.MixerConnection = await MixerConnection.ConnectViaShortCode(clientID, scopes, codeCallback);
            return (MixerAPIHandler.MixerConnection != null);
        }

        public static async Task<bool> InitializeChatClient(ChannelModel channel)
        {
            MixerAPIHandler.ChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel);
            if (await MixerAPIHandler.ChatClient.Connect() && await MixerAPIHandler.ChatClient.Authenticate())
            {
                return true;
            }
            MixerAPIHandler.ChatClient = null;
            return false;
        }
    }
}
