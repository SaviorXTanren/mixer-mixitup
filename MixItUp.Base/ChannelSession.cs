using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Models;
using MixItUp.Base.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.XSplit;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.channel__details__self,
            OAuthClientScopeEnum.channel__update__self,
            OAuthClientScopeEnum.channel__analytics__self,

            OAuthClientScopeEnum.interactive__manage__self,
            OAuthClientScopeEnum.interactive__robot__self,

            OAuthClientScopeEnum.user__details__self,
        };

        public static readonly List<OAuthClientScopeEnum> ModeratorScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
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
        };

        public static event EventHandler OnDisconectionOccurred;

        public static MixerConnectionWrapper Connection { get; private set; }
        public static MixerConnectionWrapper BotConnection { get; private set; }

        public static PrivatePopulatedUserModel User { get; private set; }
        public static PrivatePopulatedUserModel BotUser { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static ChannelSettings Settings { get; private set; }

        public static ChatClientWrapper Chat { get; private set; }
        public static ChatClientWrapper BotChat { get; private set; }
        public static InteractiveClientWrapper Interactive { get; private set; }
        public static ConstellationClientWrapper Constellation { get; private set; }

        public static OverlayWebServer OverlayServer { get; private set; }
        public static OBSWebsocket OBSWebsocket { get; private set; }
        public static XSplitWebServer XSplitServer { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }
        public static LockedDictionary<uint, UserViewModel> ChatUsers { get; private set; }
        public static LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public static bool JoinGameQueueEnabled { get; set; }
        public static LockedList<UserViewModel> JoinGameQueue { get; private set; }

        public static GiveawayItemModel Giveaway { get; set; }

        public static LockedDictionary<string, int> Counters { get; private set; }

        public static async Task<bool> Initialize(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.GetClientID(), scopes);
            if (connection != null)
            {
                ChannelSession.Connection = new MixerConnectionWrapper(connection);
                return await ChannelSession.InitializeInternal(channelName);
            }
            return false;
        }

        public static async Task<bool> Initialize(ChannelSettings settings)
        {
            bool result = false;

            ChannelSession.Settings = settings;
            ChannelSession.Settings.Initialize();

            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.OAuthToken);
                if (connection != null)
                {
                    ChannelSession.Connection = new MixerConnectionWrapper(connection);
                }
            }
            catch (RestServiceRequestException)
            {
                result = await ChannelSession.Initialize(ChannelSession.StreamerScopes, null);
            }
            
            if (settings.BotOAuthToken != null)
            {
                try
                {
                    MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.BotOAuthToken);
                    if (connection != null)
                    {
                        ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                    }
                }
                catch (RestServiceRequestException)
                {
                    settings.BotOAuthToken = null;
                }
            }

            if (ChannelSession.Connection != null)
            {
                result = await ChannelSession.InitializeInternal();
                if (result && ChannelSession.BotConnection != null)
                {
                    result = await ChannelSession.InitializeBotInternal();
                }
            }

            return result;
        }

        public static async Task<bool> InitializeBot(Action<OAuthShortCodeModel> callback)
        {
            MixerConnection connection = await MixerConnection.ConnectViaShortCode(ChannelSession.GetClientID(), ChannelSession.BotScopes, callback);
            if (connection != null)
            {
                ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                return (await ChannelSession.InitializeBotInternal() && await ChannelSession.InitializeBotChatClient());
            }
            return false;
        }

        public static async Task<bool> InitializeChatClient()
        {
            ChannelSession.CheckMixerConnection();

            if (ChannelSession.Chat != null)
            {
                ChannelSession.Chat.Client.OnDisconnectOccurred -= ChatClient_OnDisconnectOccurred;
                await ChannelSession.Chat.Disconnect();
            }

            ChannelSession.Chat = ChannelSession.BotChat = await ChannelSession.Connection.CreateChatClient(ChannelSession.Channel);
            if (await ChannelSession.Chat.ConnectAndAuthenticate())
            {
                ChannelSession.Chat.Client.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;

                if (ChannelSession.BotConnection != null)
                {
                    return await ChannelSession.InitializeBotChatClient();
                }
                return true;
            }

            ChannelSession.Chat = ChannelSession.BotChat = null;
            return false;
        }

        public static async Task<bool> InitializeBotChatClient()
        {
            if (ChannelSession.BotChat != null && ChannelSession.BotChat != ChannelSession.Chat)
            {
                ChannelSession.BotChat.Client.OnDisconnectOccurred -= BotChatClient_OnDisconnectOccurred;
                await ChannelSession.BotChat.Disconnect();
            }

            ChannelSession.BotChat = await ChannelSession.BotConnection.CreateChatClient(ChannelSession.Channel);
            if (await ChannelSession.BotChat.ConnectAndAuthenticate())
            {
                ChannelSession.BotChat.Client.OnDisconnectOccurred += BotChatClient_OnDisconnectOccurred;
                return true;
            }

            ChannelSession.BotChat = ChannelSession.Chat;
            return false;
        }

        public static async Task<bool> InitializeConstellationClient()
        {
            ChannelSession.CheckMixerConnection();

            ChannelSession.Constellation = await ChannelSession.Connection.CreateConstellationClient();
            if (await ChannelSession.Constellation.Connect())
            {
                ChannelSession.Constellation.Client.OnDisconnectOccurred += ConstellationClient_OnDisconnectOccurred;
                return true;
            }

            ChannelSession.Constellation = null;
            return false;
        }

        public static bool InitializeOverlayServer()
        {
            if (ChannelSession.OverlayServer == null)
            {
                ChannelSession.OverlayServer = new OverlayWebServer("http://localhost:8111/");
            }
            return true;
        }

        public static async Task<bool> InitializeOBSWebsocket()
        {
            if (ChannelSession.OBSWebsocket == null)
            {
                ChannelSession.OBSWebsocket = new OBSWebsocket();

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                bool connected = false;

                Task t = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ChannelSession.OBSWebsocket.Connect(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                        connected = true;
                    }
                    catch (Exception) { }
                }, tokenSource.Token);

                await Task.Delay(2000);
                tokenSource.Cancel();

                if (!connected)
                {
                    ChannelSession.OBSWebsocket = null;
                }
                return connected;
            }
            return false;
        }

        public static bool InitializeXSplitServer()
        {
            if (ChannelSession.XSplitServer == null)
            {
                ChannelSession.XSplitServer = new XSplitWebServer("http://localhost:8201/");
                ChannelSession.XSplitServer.Start();
            }
            return true;
        }

        public static async Task<bool> ConnectInteractiveClient(InteractiveGameListingModel game)
        {
            ChannelSession.CheckMixerConnection();

            ChannelSession.Interactive = await ChannelSession.Connection.CreateInteractiveClient(ChannelSession.Channel, game);
            if (await ChannelSession.Interactive.ConnectAndReady())
            {
                ChannelSession.Interactive.Client.OnDisconnectOccurred += InteractiveClient_OnDisconnectOccurred;
                return true;
            }

            ChannelSession.Interactive = null;
            return false;
        }

        public static void DisconnectBot() { ChannelSession.BotConnection = null; }

        public static async Task<bool> DisconnectInteractiveClient()
        {
            ChannelSession.CheckMixerConnection();

            if (ChannelSession.Interactive == null)
            {
                throw new InvalidOperationException("Interactive client is not connected");
            }

            ChannelSession.Interactive.Client.OnDisconnectOccurred -= InteractiveClient_OnDisconnectOccurred;
            await ChannelSession.Interactive.Disconnect();

            ChannelSession.Interactive = null;
            return true;
        }

        public static void DisconnectOverlayServer()
        {
            if (ChannelSession.OverlayServer != null)
            {
                ChannelSession.OverlayServer.Close();
                ChannelSession.OverlayServer = null;
            }
        }

        public static void DisconnectOBSStudio()
        {
            if (ChannelSession.OBSWebsocket != null)
            {
                ChannelSession.OBSWebsocket.Disconnect();
                ChannelSession.OBSWebsocket = null;
            }
        }

        public static void DisconnectXSplitServer()
        {
            if (ChannelSession.XSplitServer != null)
            {
                ChannelSession.XSplitServer.End();
            }
        }

        //public static async Task Close()
        //{
        //    ChannelSession.DisconnectOverlayServer();

        //    ChannelSession.DisconnectOBSStudio();

        //    ChannelSession.DisconnectXSplitServer();

        //    if (ChannelSession.ChatClient != null)
        //    {
        //        ChannelSession.ChatClient.OnDisconnectOccurred -= ChatClient_OnDisconnectOccurred;
        //        await ChannelSession.ChatClient.Disconnect();
        //        ChannelSession.ChatClient = null;
        //    }

        //    if (ChannelSession.BotChatClient != null)
        //    {
        //        ChannelSession.BotChatClient.OnDisconnectOccurred -= BotChatClient_OnDisconnectOccurred;
        //        await ChannelSession.BotChatClient.Disconnect();
        //        ChannelSession.BotChatClient = null;
        //    }

        //    if (ChannelSession.InteractiveClient != null)
        //    {
        //        await ChannelSession.DisconnectInteractiveClient();
        //    }

        //    if (ChannelSession.ConstellationClient != null)
        //    {
        //        ChannelSession.ConstellationClient.OnDisconnectOccurred -= ConstellationClient_OnDisconnectOccurred;
        //        await ChannelSession.ConstellationClient.Disconnect();
        //        ChannelSession.ConstellationClient = null;
        //    }
        //}

        public static async Task SaveSettings() { await ChannelSession.Settings.Save(); }

        public static async Task RefreshUser()
        {
            if (ChannelSession.User != null)
            {
                ChannelSession.User = await ChannelSession.Connection.GetCurrentUser();
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.Channel != null)
            {
                ChannelSession.Channel = await ChannelSession.Connection.GetChannel(ChannelSession.Channel.user.username);
            }
        }

        public static UserViewModel GetCurrentUser() { return new UserViewModel(User); }

        private static async Task<bool> InitializeInternal(string channelName = null)
        {
            PrivatePopulatedUserModel user = await ChannelSession.Connection.GetCurrentUser();
            if (user != null)
            {
                ExpandedChannelModel channel = await ChannelSession.Connection.GetChannel((channelName == null) ? user.username : channelName);
                if (channel != null)
                {
                    ChannelSession.User = user;
                    ChannelSession.Channel = channel;

                    ChannelSession.PreMadeChatCommands = new List<PreMadeChatCommand>();
                    ChannelSession.ChatUsers = new LockedDictionary<uint, UserViewModel>();
                    ChannelSession.InteractiveUsers = new LockedDictionary<string, InteractiveParticipantModel>();
                    ChannelSession.JoinGameQueue = new LockedList<UserViewModel>();

                    ChannelSession.Giveaway = new GiveawayItemModel();

                    ChannelSession.Counters = new LockedDictionary<string, int>();
                    
                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = new ChannelSettings(channel, (channelName == null));
                    }
                    await ChannelSession.SaveSettings();

                    await ChannelSession.Settings.SaveBackup();

                    return true;
                }
            }
            return false;
        }

        private static async Task<bool> InitializeBotInternal()
        {
            PrivatePopulatedUserModel user = await ChannelSession.BotConnection.GetCurrentUser();
            if (user != null)
            {
                ChannelSession.BotUser = user;

                await ChannelSession.SaveSettings();

                return true;
            }
            return false;
        }

        private static void CheckMixerConnection()
        {
            if (ChannelSession.Connection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }
        }

        private static string GetClientID()
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }
            return clientID;
        }

        private static async void ChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();
            while (!await ChannelSession.Chat.ConnectAndAuthenticate())
            {
                await Task.Delay(2000);
            }
        }

        private static async void BotChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();
            while (!await ChannelSession.BotChat.ConnectAndAuthenticate())
            {
                await Task.Delay(2000);
            }
        }

        private static async void ConstellationClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();
            while (!await ChannelSession.Constellation.Connect())
            {
                await Task.Delay(2000);
            }
        }

        private static async void InteractiveClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();
            while (!await ChannelSession.Interactive.ConnectAndReady())
            {
                await Task.Delay(2000);
            }
        }

        private static void DisconnectionOccurred()
        {
            if (ChannelSession.OnDisconectionOccurred != null)
            {
                ChannelSession.OnDisconectionOccurred(null, new EventArgs());
            }
        }
    }
}
