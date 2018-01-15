using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Client;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MixItUp.Desktop")]

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public const string ClientID = "5e3140d0719f5842a09dd2700befbfc100b5a246e35f2690";

        public const string DefaultOBSStudioConnection = "ws://127.0.0.1:4444";

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

        public static MixerConnectionWrapper Connection { get; private set; }
        public static MixerConnectionWrapper BotConnection { get; private set; }

        public static PrivatePopulatedUserModel User { get; private set; }
        public static PrivatePopulatedUserModel BotUser { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static IChannelSettings Settings { get; private set; }

        public static ChatClientWrapper Chat { get; private set; }
        public static InteractiveClientWrapper Interactive { get; private set; }
        public static ConstellationClientWrapper Constellation { get; private set; }

        public static ServicesHandlerBase Services { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }

        public static bool GameQueueEnabled { get; set; }
        public static LockedList<UserViewModel> GameQueue { get; private set; }

        public static LockedDictionary<string, int> Counters { get; private set; }

        public static IEnumerable<PermissionsCommandBase> AllChatCommands
        {
            get
            {
                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(ChannelSession.PreMadeChatCommands);
                commands.AddRange(ChannelSession.Settings.ChatCommands);
                commands.AddRange(ChannelSession.Settings.GameCommands);
                return commands;
            }
        }

        public static bool IsStreamer
        {
            get
            {
                if (ChannelSession.User != null && ChannelSession.Channel != null)
                {
                    return ChannelSession.User.id == ChannelSession.Channel.user.id;
                }
                return false;
            }
        }

        public static void Initialize(ServicesHandlerBase serviceHandler)
        {
            ChannelSession.Services = serviceHandler;
            ChannelSession.Chat = new ChatClientWrapper();
        }

        public static async Task<bool> ConnectUser(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, scopes, false, "LoginRedirectPage.html");
            if (connection != null)
            {
                ChannelSession.Connection = new MixerConnectionWrapper(connection);
                return await ChannelSession.InitializeInternal(channelName);
            }
            return false;
        }

        public static async Task<bool> ConnectUser(IChannelSettings settings)
        {
            bool result = false;

            ChannelSession.Settings = settings;

            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.OAuthToken);
                if (connection != null)
                {
                    ChannelSession.Connection = new MixerConnectionWrapper(connection);
                    result = await ChannelSession.InitializeInternal();
                }
            }
            catch (RestServiceRequestException)
            {
                result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes, null);
            }

            return result;
        }

        public static async Task<bool> ConnectBot(Action<OAuthShortCodeModel> callback)
        {
            MixerConnection connection = await MixerConnection.ConnectViaShortCode(ChannelSession.ClientID, ChannelSession.BotScopes, callback);
            if (connection != null)
            {
                ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                return await ChannelSession.InitializeBotInternal();
            }
            return false;
        }

        public static async Task<bool> ConnectBot(IChannelSettings settings)
        {
            bool result = true;

            if (settings.BotOAuthToken != null)
            {
                try
                {
                    MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.BotOAuthToken);
                    if (connection != null)
                    {
                        ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                        result = await ChannelSession.InitializeBotInternal();
                    }
                }
                catch (RestServiceRequestException)
                {
                    settings.BotOAuthToken = null;
                    return false;
                }
            }

            return result;
        }

        public static async Task DisconnectBot()
        {
            ChannelSession.BotConnection = null;
            await ChannelSession.Chat.DisconnectBot();
        }

        public static async Task<bool> ConnectConstellation()
        {
            ChannelSession.Constellation = await ChannelSession.Connection.CreateConstellationClient();
            if (await ChannelSession.Constellation.Connect())
            {
                ChannelSession.Constellation.Client.OnDisconnectOccurred += ConstellationClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    ChannelSession.Constellation.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                    ChannelSession.Constellation.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                    ChannelSession.Constellation.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                }
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
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    ChannelSession.Constellation.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    ChannelSession.Constellation.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    ChannelSession.Constellation.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }
                await ChannelSession.Constellation.Disconnect();
                ChannelSession.Constellation = null;
            }
        }

        public static async Task<bool> ConnectInteractive(InteractiveGameListingModel game)
        {
            ChannelSession.Interactive = await ChannelSession.Connection.CreateInteractiveClient(ChannelSession.Channel, game);
            if (await ChannelSession.Interactive.ConnectAndReady())
            {
                ChannelSession.Interactive.Client.OnDisconnectOccurred += InteractiveClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    ChannelSession.Interactive.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                    ChannelSession.Interactive.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                    ChannelSession.Interactive.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                }
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
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    ChannelSession.Interactive.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    ChannelSession.Interactive.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    ChannelSession.Interactive.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }
                await ChannelSession.Interactive.Disconnect();

                ChannelSession.Interactive = null;
            }

            return true;
        }

        public static async Task Close()
        {
            await ChannelSession.Services.Close();
        }

        public static async Task SaveSettings()
        {
            await ChannelSession.Services.Settings.Save(ChannelSession.Settings);
        }

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

        public static UserViewModel GetCurrentUser()
        {
            return new UserViewModel(User);
        }

        public static void DisconnectionOccurred()
        {
            Logger.Log("Service disconnection occurred, attempting to reconnect now...");
        }

        public static void ReconnectionOccurred()
        {
            Logger.Log("Service reconnection successful");
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
                    ChannelSession.GameQueue = new LockedList<UserViewModel>();

                    ChannelSession.Counters = new LockedDictionary<string, int>();

                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = ChannelSession.Services.Settings.Create(channel, (channelName == null));
                    }
                    await ChannelSession.Services.Settings.Initialize(ChannelSession.Settings);

                    ChannelSession.Connection.Initialize();

                    await ChannelSession.Chat.Connect();

                    GlobalEvents.OnRankChanged += GlobalEvents_OnRankChanged;

                    await ChannelSession.SaveSettings();
                    await ChannelSession.Services.Settings.SaveBackup(ChannelSession.Settings);

                    await Logger.LogAnalyticsUsage("LogIn", "Desktop");

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

                ChannelSession.BotConnection.Initialize();

                await ChannelSession.Chat.ConnectBot();

                await ChannelSession.SaveSettings();

                return true;
            }
            return false;
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



        private static async void GlobalEvents_OnRankChanged(object sender, UserCurrencyDataViewModel currency)
        {
            if (currency.Currency.RankChangedCommand != null && ChannelSession.Chat.ChatUsers.ContainsKey(currency.User.ID) == true)
            {
                var user = ChannelSession.Chat.ChatUsers[currency.User.ID];
                await currency.Currency.RankChangedCommand.Perform(user);
            }
        }

        private static void WebSocketClient_OnMethodOccurred(object sender, MethodPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Method: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.method, e.arguments, e.parameters));
        }

        private static void WebSocketClient_OnReplyOccurred(object sender, ReplyPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Reply: {0} - {1} - {2} - {3} - {4}" + Environment.NewLine, e.id, e.type, e.result, e.error, e.data));
        }

        private static void WebSocketClient_OnEventOccurred(object sender, EventPacket e)
        {
            Logger.Log(string.Format(Environment.NewLine + "WebSocket Event: {0} - {1} - {2} - {3}" + Environment.NewLine, e.id, e.type, e.eventName, e.data));
        }
    }
}