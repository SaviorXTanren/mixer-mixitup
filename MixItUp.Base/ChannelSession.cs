using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace MixItUp
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
        public static event EventHandler OnReconectionOccurred;

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

        public static ServicesHandlerBase Services { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }
        public static LockedDictionary<uint, UserViewModel> ChatUsers { get; private set; }
        public static LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public static bool GameQueueEnabled { get; set; }
        public static LockedList<UserViewModel> GameQueue { get; private set; }
        public static event EventHandler OnGameQueueUpdated;

        public static GiveawayItemViewModel Giveaway { get; set; }

        public static LockedDictionary<string, int> Counters { get; private set; }

        public static async Task<bool> ConnectUser(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.GetClientID(), scopes);
            if (connection != null)
            {
                ChannelSession.Connection = new MixerConnectionWrapper(connection);
                return await ChannelSession.InitializeInternal(channelName);
            }
            return false;
        }

        public static async Task<bool> ConnectUser(ChannelSettings settings)
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
                result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes, null);
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

        public static async Task<bool> ConnectBot(Action<OAuthShortCodeModel> callback)
        {
            MixerConnection connection = await MixerConnection.ConnectViaShortCode(ChannelSession.GetClientID(), ChannelSession.BotScopes, callback);
            if (connection != null)
            {
                ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                return (await ChannelSession.InitializeBotInternal() && await ChannelSession.ConnectBotChat());
            }
            return false;
        }

        public static async Task DisconnectBot()
        {
            ChannelSession.BotConnection = null;
            await ChannelSession.DisconnectBotChat();
        }

        public static async Task<bool> ConnectChat()
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
                    return await ChannelSession.ConnectBotChat();
                }
                return true;
            }

            ChannelSession.Chat = ChannelSession.BotChat = null;
            return false;
        }

        public static async Task DisconnectChat()
        {
            if (ChannelSession.Chat != null)
            {
                ChannelSession.Chat.Client.OnDisconnectOccurred -= ChatClient_OnDisconnectOccurred;
                await ChannelSession.Chat.Disconnect();
                ChannelSession.Chat = null;
            }
        }

        public static async Task<bool> ConnectBotChat()
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

        public static async Task DisconnectBotChat()
        {
            if (ChannelSession.BotChat != null && ChannelSession.BotChat != ChannelSession.Chat)
            {
                ChannelSession.BotChat.Client.OnDisconnectOccurred -= BotChatClient_OnDisconnectOccurred;
                await ChannelSession.BotChat.Disconnect();
            }

            ChannelSession.BotChat = ChannelSession.Chat;
        }

        public static async Task<bool> ConnectConstellation()
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

        public static async Task DisconnectConstellation()
        {
            if (ChannelSession.Constellation != null)
            {
                ChannelSession.Constellation.Client.OnDisconnectOccurred -= ConstellationClient_OnDisconnectOccurred;
                await ChannelSession.Constellation.Disconnect();
                ChannelSession.Constellation = null;
            }
        }

        public static async Task<bool> ConnectInteractive(InteractiveGameListingModel game)
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

        public static async Task<bool> DisconnectInteractive()
        {
            if (ChannelSession.Interactive != null)
            {
                ChannelSession.Interactive.Client.OnDisconnectOccurred -= InteractiveClient_OnDisconnectOccurred;
                await ChannelSession.Interactive.Disconnect();

                ChannelSession.Interactive = null;
            }

            return true;
        }

        public static void AssignServicesHandler(ServicesHandlerBase serviceHandler) { ChannelSession.Services = serviceHandler; }

        public static async Task Close()
        {
            await ChannelSession.Services.Close();
        }

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

        public static void GameQueueUpdated()
        {
            if (ChannelSession.OnGameQueueUpdated != null)
            {
                ChannelSession.OnGameQueueUpdated(null, new EventArgs());
            }
        }

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
                    ChannelSession.GameQueue = new LockedList<UserViewModel>();

                    ChannelSession.Giveaway = new GiveawayItemViewModel();

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

            do
            {
                await ChannelSession.DisconnectChat();

                await Task.Delay(2000);

            } while (!await ChannelSession.ConnectChat());

            ChannelSession.ReconnectionOccurred();
        }

        private static async void BotChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            do
            {
                await ChannelSession.DisconnectBotChat();

                await Task.Delay(2000);

            } while (!await ChannelSession.ConnectBotChat());

            ChannelSession.ReconnectionOccurred();
        }

        private static async void ConstellationClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            do
            {
                await ChannelSession.DisconnectConstellation();

                await Task.Delay(2000);

            } while (!await ChannelSession.ConnectConstellation());

            ChannelSession.ReconnectionOccurred();
        }

        private static async void InteractiveClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            InteractiveGameListingModel game = ChannelSession.Interactive.Client.InteractiveGame;
            do
            {
                await ChannelSession.DisconnectInteractive();

                await Task.Delay(2000);

            } while (!await ChannelSession.ConnectInteractive(game));

            ChannelSession.ReconnectionOccurred();
        }

        private static void DisconnectionOccurred()
        {
            if (ChannelSession.OnDisconectionOccurred != null)
            {
                ChannelSession.OnDisconectionOccurred(null, new EventArgs());
            }
        }

        private static void ReconnectionOccurred()
        {
            if (ChannelSession.OnReconectionOccurred != null)
            {
                ChannelSession.OnReconectionOccurred(null, new EventArgs());
            }
        }
    }
}
