using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using MixItUp.Base.Commands;
using MixItUp.Base.Overlay;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static ChannelSettings ChannelSettings { get; private set; }

        public static async Task<bool> InitializeMixerClient(string clientID, IEnumerable<ClientScopeEnum> scopes)
        {
            MixerAPIHandler.MixerConnection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(clientID, scopes);
            if (MixerAPIHandler.MixerConnection != null)
            {
                MixerAPIHandler.ChannelSettings = new ChannelSettings();
                await MixerAPIHandler.LoadSettings();

                if (MixerAPIHandler.OverlayServer == null)
                {
                    MixerAPIHandler.OverlayServer = new OverlayWebServer("http://localhost:8001/");
                    MixerAPIHandler.OverlayServer.Start();
                }
            }

            return (MixerAPIHandler.MixerConnection != null);
        }

        public static async Task LoadSettings()
        {
            if (File.Exists(ChannelSettings.ChannelSettingsFileName))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(ChannelSettings.ChannelSettingsFileName)))
                {
                    MixerAPIHandler.ChannelSettings = SerializerHelper.Deserialize<ChannelSettings>(await reader.ReadToEndAsync());
                }

                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.ChatCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.InteractiveCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.EventCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.TimerCommands) { command.DeserializeActions(); }
            }
        }

        public static async Task SaveSettings()
        {
            if (MixerAPIHandler.ChannelSettings != null)
            {
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.ChatCommands) { command.SerializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.InteractiveCommands) { command.SerializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.EventCommands) { command.SerializeActions(); }
                foreach (CommandBase command in MixerAPIHandler.ChannelSettings.TimerCommands) { command.SerializeActions(); }

                using (StreamWriter writer = new StreamWriter(File.OpenWrite(ChannelSettings.ChannelSettingsFileName)))
                {
                    string data = SerializerHelper.Serialize<ChannelSettings>(MixerAPIHandler.ChannelSettings);
                    await writer.WriteAsync(data);
                }
            }
        }

        public static async Task<bool> InitializeChatClient(ChannelModel channel)
        {
            if (MixerAPIHandler.MixerConnection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }

            MixerAPIHandler.ChatClient = await ChatClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel);
            if (await MixerAPIHandler.ChatClient.Connect() && await MixerAPIHandler.ChatClient.Authenticate())
            {
                return true;
            }
            MixerAPIHandler.ChatClient = null;

            return false;
        }

        public static async Task<bool> InitializeInteractiveClient(ChannelModel channel, InteractiveGameListingModel game)
        {
            if (MixerAPIHandler.MixerConnection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }

            MixerAPIHandler.InteractiveClient = await InteractiveClient.CreateFromChannel(MixerAPIHandler.MixerConnection, channel, game);
            if (await MixerAPIHandler.InteractiveClient.Connect() && await MixerAPIHandler.InteractiveClient.Ready())
            {
                return true;
            }
            MixerAPIHandler.InteractiveClient = null;
            return false;
        }

        public static async Task<bool> InitializeConstellationClient()
        {
            if (MixerAPIHandler.MixerConnection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }

            MixerAPIHandler.ConstellationClient = await ConstellationClient.Create(MixerAPIHandler.MixerConnection);
            if (await MixerAPIHandler.ConstellationClient.Connect())
            {
                return true;
            }
            MixerAPIHandler.ConstellationClient = null;

            return false;
        }

        public static async Task Close()
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

            await MixerAPIHandler.SaveSettings();
        }
    }
}
