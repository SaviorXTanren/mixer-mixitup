using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Services;
using MixItUp.Base.Statistics;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static StatisticsTracker Statistics { get; private set; }

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
                return commands.Where(c => c.IsEnabled);
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
            ChannelSession.PreMadeChatCommands = new List<PreMadeChatCommand>();
            ChannelSession.GameQueue = new LockedList<UserViewModel>();

            ChannelSession.Counters = new LockedDictionary<string, int>();

            ChannelSession.Services = serviceHandler;

            ChannelSession.Chat = new ChatClientWrapper();
            ChannelSession.Constellation = new ConstellationClientWrapper();
            ChannelSession.Interactive = new InteractiveClientWrapper();

            ChannelSession.Statistics = new StatisticsTracker();
        }

        public static async Task<bool> ConnectUser(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, scopes, false, loginSuccessHtmlPageFilePath: "LoginRedirectPage.html");
                if (connection != null)
                {
                    ChannelSession.Connection = new MixerConnectionWrapper(connection);
                    return await ChannelSession.InitializeInternal(channelName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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
            catch (RestServiceRequestException ex)
            {
                Logger.Log(ex);
                result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes, null);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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

        public static async Task Close()
        {
            await ChannelSession.Services.Close();

            await ChannelSession.Chat.Disconnect();
            await ChannelSession.DisconnectBot();

            await ChannelSession.Constellation.Disconnect();
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
            UserViewModel user = new UserViewModel(ChannelSession.User);
            if (ChannelSession.Channel.user.id.Equals(user.ID))
            {
                user.Roles.Add(UserRole.Streamer);
            }
            else
            {
                user.Roles.Add(UserRole.Mod);
            }
            return user;
        }

        public static void DisconnectionOccurred(string service)
        {
            Logger.Log(service + " Service disconnection occurred, attempting to reconnect now...");
        }

        public static void ReconnectionOccurred(string service)
        {
            Logger.Log(service + " Service reconnection successful");
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

                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = ChannelSession.Services.Settings.Create(channel, (channelName == null));
                    }
                    await ChannelSession.Services.Settings.Initialize(ChannelSession.Settings);

                    ChannelSession.Connection.Initialize();

                    if (!await ChannelSession.Chat.Connect() || !await ChannelSession.Constellation.Connect())
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
                    {
                        await ChannelSession.Services.InitializeOBSWebsocket();
                    }

                    if (ChannelSession.Settings.EnableXSplitConnection)
                    {
                        await ChannelSession.Services.InitializeXSplitServer();
                    }

                    if (ChannelSession.Settings.EnableOverlay)
                    {
                        await ChannelSession.Services.InitializeOverlayServer();
                    }

                    if (ChannelSession.Settings.EnableDeveloperAPI)
                    {
                        await ChannelSession.Services.InitializeDeveloperAPI();
                    }

                    if (ChannelSession.Settings.StreamlabsOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeStreamlabs();
                    }

                    await ChannelSession.Services.Settings.CleanUpData(ChannelSession.Settings);

                    await ChannelSession.SaveSettings();
                    await ChannelSession.Services.Settings.SaveBackup(ChannelSession.Settings);

                    await Logger.LogAnalyticsUsage("LogIn", "Desktop");

                    GlobalEvents.OnRankChanged += GlobalEvents_OnRankChanged;

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

        private static async void GlobalEvents_OnRankChanged(object sender, UserCurrencyDataViewModel currency)
        {
            if (currency.Currency.RankChangedCommand != null && ChannelSession.Chat.ChatUsers.ContainsKey(currency.User.ID) == true)
            {
                var user = ChannelSession.Chat.ChatUsers[currency.User.ID];
                await currency.Currency.RankChangedCommand.Perform(user);
            }
        }
    }
}