using Mixer.Base;
using Mixer.Base.Model.Channel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class MixerAPIHandler
    {
        public static MixerClient MixerClient { get; private set; }
        public static ChatClient ChatClient { get; private set; }

        public static async Task InitializeMixerClient(string clientID, IEnumerable<ClientScopeEnum> scopes, Action<string> codeCallback)
        {
            MixerAPIHandler.MixerClient = await MixerClient.ConnectViaShortCode(clientID, scopes, codeCallback);
        }

        public static async Task<bool> InitializeChatClient(ChannelModel channel)
        {
            MixerAPIHandler.ChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.MixerClient, channel);
            if (await MixerAPIHandler.ChatClient.Connect() && await MixerAPIHandler.ChatClient.Authenticate())
            {
                return true;
            }
            MixerAPIHandler.ChatClient = null;
            return false;
        }
    }
}
