using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using MixItUp.Base.Overlay;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class MixerAPIHandler
    {
        public static MixerConnection MixerConnection { get; private set; }

        public static ChatClient ChatClient { get; private set; }
        public static InteractiveClient InteractiveClient { get; private set; }
        public static ConstellationClient ConstellationClient { get; private set; }

        public static OverlayWebServer OverlayServer { get; private set; }

        public static async Task<bool> InitializeMixerClient(string clientID, IEnumerable<OAuthClientScopeEnum> scopes)
        {
            MixerAPIHandler.MixerConnection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(clientID, scopes);
            return (MixerAPIHandler.MixerConnection != null);
        }

        public static async Task<bool> InitializeChatClient(ChannelModel channel)
        {
            MixerAPIHandler.CheckMixerConnection();

            MixerAPIHandler.ChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel);
            if (await MixerAPIHandler.ChatClient.Connect() && await MixerAPIHandler.ChatClient.Authenticate())
            {
                return true;
            }

            MixerAPIHandler.ChatClient = null;
            return false;
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
