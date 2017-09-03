using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Overlay;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class MixerAPIHandler
    {
        public static MixerConnection MixerConnection { get; private set; }
        public static MixerConnection BotConnection { get; private set; }

        public static ChatClient ChatClient { get; private set; }
        public static ChatClient BotChatClient { get; private set; }
        public static InteractiveClient InteractiveClient { get; private set; }
        public static ConstellationClient ConstellationClient { get; private set; }

        public static OverlayWebServer OverlayServer { get; private set; }

        public static async Task<bool> InitializeMixerClient(string clientID, IEnumerable<OAuthClientScopeEnum> scopes)
        {
            MixerAPIHandler.MixerConnection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(clientID, scopes);
            return (MixerAPIHandler.MixerConnection != null);
        }

        public static async Task<bool> InitializeBotConnection(string clientID, Action<OAuthShortCodeModel> callback)
        {
            MixerAPIHandler.BotConnection = await MixerConnection.ConnectViaShortCode(clientID, new List<OAuthClientScopeEnum>()
            {
                OAuthClientScopeEnum.chat__bypass_links,
                OAuthClientScopeEnum.chat__bypass_slowchat,
                OAuthClientScopeEnum.chat__chat,
                OAuthClientScopeEnum.chat__connect,
                OAuthClientScopeEnum.chat__edit_options,
                OAuthClientScopeEnum.chat__giveaway_start,
                OAuthClientScopeEnum.chat__poll_start,
                OAuthClientScopeEnum.chat__poll_vote,
                OAuthClientScopeEnum.chat__whisper,

                OAuthClientScopeEnum.user__details__self,
            }, callback);
            return (MixerAPIHandler.BotConnection != null);
        }

        public static async Task<bool> InitializeChatClient(ChannelModel channel)
        {
            MixerAPIHandler.CheckMixerConnection();

            MixerAPIHandler.ChatClient = MixerAPIHandler.BotChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel);
            if (!(await MixerAPIHandler.ChatClient.Connect() && await MixerAPIHandler.ChatClient.Authenticate()))
            {
                MixerAPIHandler.ChatClient = MixerAPIHandler.BotChatClient = null;
                return false;
            }

            if (MixerAPIHandler.BotConnection != null)
            {
                MixerAPIHandler.BotChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.BotConnection, channel);
                if (!(await MixerAPIHandler.BotChatClient.Connect() && await MixerAPIHandler.BotChatClient.Authenticate()))
                {
                    MixerAPIHandler.BotChatClient = null;
                    return false;
                }            
            }

            return true;
        }

        public static async Task<bool> InitializeConstellationClient()
        {
            MixerAPIHandler.CheckMixerConnection();

            MixerAPIHandler.ConstellationClient = await ConstellationClient.Create(MixerAPIHandler.MixerConnection);
            if (await MixerAPIHandler.ConstellationClient.Connect())
            {
                return true;
            }

            MixerAPIHandler.ConstellationClient = null;
            return false;
        }

        public static bool InitializeOverlayServer()
        {
            if (MixerAPIHandler.OverlayServer == null)
            {
                MixerAPIHandler.OverlayServer = new OverlayWebServer("http://localhost:8001/");
                MixerAPIHandler.OverlayServer.Start();
            }
            return true;
        }

        public static async Task<bool> ConnectInteractiveClient(ChannelModel channel, InteractiveGameListingModel game)
        {
            MixerAPIHandler.CheckMixerConnection();

            MixerAPIHandler.InteractiveClient = await InteractiveClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel, game);
            if (await MixerAPIHandler.InteractiveClient.Connect() && await MixerAPIHandler.InteractiveClient.Ready())
            {
                return true;
            }

            MixerAPIHandler.InteractiveClient = null;
            return false;
        }

        public static async Task<bool> DisconnectInteractiveClient()
        {
            MixerAPIHandler.CheckMixerConnection();

            if (MixerAPIHandler.InteractiveClient == null)
            {
                throw new InvalidOperationException("Interactive client is not connected");
            }
            await MixerAPIHandler.InteractiveClient.Disconnect();

            MixerAPIHandler.InteractiveClient = null;
            return true;
        }

        public static void Close()
        {
            if (MixerAPIHandler.OverlayServer != null)
            {
                MixerAPIHandler.OverlayServer.End();
            }

            if (MixerAPIHandler.ChatClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                MixerAPIHandler.ChatClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (MixerAPIHandler.InteractiveClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                MixerAPIHandler.InteractiveClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (MixerAPIHandler.ConstellationClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                MixerAPIHandler.ConstellationClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private static void CheckMixerConnection()
        {
            if (MixerAPIHandler.MixerConnection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }
        }
    }
}
