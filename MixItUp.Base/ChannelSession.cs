using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Statistics;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MixItUp.Desktop")]

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public const string ClientID = "5e3140d0719f5842a09dd2700befbfc100b5a246e35f2690";

        public const string DefaultOBSStudioConnection = "ws://127.0.0.1:4444";
        public const string DefaultOvrStreamConnection = "ws://127.0.0.1:8023";

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

            OAuthClientScopeEnum.channel__clip__create__self,
            OAuthClientScopeEnum.channel__details__self,
            OAuthClientScopeEnum.channel__follow__self,
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

            OAuthClientScopeEnum.channel__follow__self,

            OAuthClientScopeEnum.user__details__self,

            OAuthClientScopeEnum.user__act_as,
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

            OAuthClientScopeEnum.user__act_as,
        };

        public static SecretManagerService SecretManager { get; internal set; }

        public static MixerConnectionService MixerStreamerConnection { get; private set; }
        public static MixerConnectionService MixerBotConnection { get; private set; }

        public static PrivatePopulatedUserModel MixerStreamerUser { get; private set; }
        public static PrivatePopulatedUserModel MixerBotUser { get; private set; }
        public static ExpandedChannelModel MixerChannel { get; private set; }

        public static IChannelSettings Settings { get; private set; }

        public static MixPlayClientWrapper Interactive { get; private set; }
        public static ConstellationClientWrapper Constellation { get; private set; }

        public static StatisticsTracker Statistics { get; private set; }

        public static ServicesHandlerBase Services { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }

        public static LockedDictionary<string, double> Counters { get; private set; }

        private static CancellationTokenSource sessionBackgroundCancellationTokenSource = new CancellationTokenSource();
        private static int sessionBackgroundTimer = 0;

        public static bool IsDebug()
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }

        public static IEnumerable<PermissionsCommandBase> AllEnabledChatCommands
        {
            get
            {
                return ChannelSession.AllChatCommands.Where(c => c.IsEnabled);
            }
        }

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

        public static IEnumerable<CommandBase> AllEnabledCommands
        {
            get
            {
                return ChannelSession.AllCommands.Where(c => c.IsEnabled);
            }
        }

        public static IEnumerable<CommandBase> AllCommands
        {
            get
            {
                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(ChannelSession.AllChatCommands);
                commands.AddRange(ChannelSession.Settings.EventCommands);
                commands.AddRange(ChannelSession.Settings.MixPlayCommands);
                commands.AddRange(ChannelSession.Settings.TimerCommands);
                commands.AddRange(ChannelSession.Settings.ActionGroupCommands);
                return commands;
            }
        }

        public static IDictionary<string, int> AllOverlayNameAndPorts
        {
            get
            {
                Dictionary<string, int> results = new Dictionary<string, int>(ChannelSession.Settings.OverlayCustomNameAndPorts);
                results.Add(ChannelSession.Services.OverlayServers.DefaultOverlayName, ChannelSession.Services.OverlayServers.DefaultOverlayPort);
                return results;
            }
        }

        public static bool IsStreamer
        {
            get
            {
                if (ChannelSession.MixerStreamerUser != null && ChannelSession.MixerChannel != null)
                {
                    return ChannelSession.MixerStreamerUser.id == ChannelSession.MixerChannel.user.id;
                }
                return false;
            }
        }

        public static void Initialize(ServicesHandlerBase serviceHandler)
        {
            try
            {
                Type mixItUpSecretsType = Type.GetType("MixItUp.Base.MixItUpSecrets");
                if (mixItUpSecretsType != null)
                {
                    ChannelSession.SecretManager = (SecretManagerService)Activator.CreateInstance(mixItUpSecretsType);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            if (ChannelSession.SecretManager == null)
            {
                ChannelSession.SecretManager = new SecretManagerService();
            }

            ChannelSession.PreMadeChatCommands = new List<PreMadeChatCommand>();

            ChannelSession.Counters = new LockedDictionary<string, double>();

            ChannelSession.Services = serviceHandler;

            ChannelSession.Constellation = new ConstellationClientWrapper();
            ChannelSession.Interactive = new MixPlayClientWrapper();

            ChannelSession.Statistics = new StatisticsTracker();
        }

        public static async Task<bool> ConnectUser(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, scopes, false, successResponse: OAuthServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    ChannelSession.MixerStreamerConnection = new MixerConnectionService(connection);
                    return await ChannelSession.InitializeInternal((channelName == null), channelName);
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
                MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(ChannelSession.Settings.OAuthToken);
                if (connection != null)
                {
                    ChannelSession.MixerStreamerConnection = new MixerConnectionService(connection);
                    result = await ChannelSession.InitializeInternal(ChannelSession.Settings.IsStreamer, ChannelSession.Settings.IsStreamer ? null : ChannelSession.Settings.Channel.token);
                }
            }
            catch (HttpRestRequestException ex)
            {
                Logger.Log(ex);
                result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes, ChannelSession.Settings.IsStreamer ? null : ChannelSession.Settings.Channel.token);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return result;
        }

        public static async Task<bool> ConnectBot()
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, ChannelSession.BotScopes, false, successResponse: OAuthServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    ChannelSession.MixerBotConnection = new MixerConnectionService(connection);
                    return await ChannelSession.InitializeBotInternal();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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
                        ChannelSession.MixerBotConnection = new MixerConnectionService(connection);
                        result = await ChannelSession.InitializeBotInternal();
                    }
                }
                catch (HttpRestRequestException)
                {
                    settings.BotOAuthToken = null;
                    return false;
                }
            }

            return result;
        }

        public static async Task DisconnectBot()
        {
            ChannelSession.MixerBotConnection = null;
            await ChannelSession.Services.Chat.MixerChatService.DisconnectBot();
        }

        public static async Task Close()
        {
            await ChannelSession.Services.Close();

            await ChannelSession.Services.Chat.MixerChatService.DisconnectStreamer();
            await ChannelSession.DisconnectBot();

            await ChannelSession.Constellation.Disconnect();
        }

        public static async Task SaveSettings()
        {
            await ChannelSession.Services.Settings.Save(ChannelSession.Settings);
        }

        public static async Task RefreshUser()
        {
            if (ChannelSession.MixerStreamerUser != null)
            {
                PrivatePopulatedUserModel user = await ChannelSession.MixerStreamerConnection.GetCurrentUser();
                if (user != null)
                {
                    ChannelSession.MixerStreamerUser = user;
                }
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.MixerChannel != null)
            {
                ExpandedChannelModel channel = await ChannelSession.MixerStreamerConnection.GetChannel(ChannelSession.MixerChannel.id);
                if (channel != null)
                {
                    ChannelSession.MixerChannel = channel;
                }
            }
        }

        public static async Task<UserViewModel> GetCurrentUser()
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByID(ChannelSession.MixerStreamerUser.id);
            if (user == null)
            {
                user = new UserViewModel(ChannelSession.MixerStreamerUser);
            }
            return user;
        }

        public static void DisconnectionOccurred(string serviceName)
        {
            Logger.Log(serviceName + " Service disconnection occurred");
            GlobalEvents.ServiceDisconnect(serviceName);
        }

        public static void ReconnectionOccurred(string serviceName)
        {
            Logger.Log(serviceName + " Service reconnection successful");
            GlobalEvents.ServiceReconnect(serviceName);
        }

        private static async Task<bool> InitializeInternal(bool isStreamer, string channelName = null)
        {
            await ChannelSession.Services.InitializeTelemetryService();

            PrivatePopulatedUserModel user = await ChannelSession.MixerStreamerConnection.GetCurrentUser();
            if (user != null)
            {
                ExpandedChannelModel channel = null;
                if (channelName == null || isStreamer)
                {
                    channel = await ChannelSession.MixerStreamerConnection.GetChannel(user.channel.id);
                }
                else
                {
                    channel = await ChannelSession.MixerStreamerConnection.GetChannel(channelName);
                }
                
                if (channel != null)
                {
                    ChannelSession.MixerStreamerUser = user;
                    ChannelSession.MixerChannel = channel;

                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = await ChannelSession.Services.Settings.Create(channel, isStreamer);
                    }
                    await ChannelSession.Services.Settings.Initialize(ChannelSession.Settings);

                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        Logger.SetLogLevel(LogLevel.Debug);
                    }

                    ChannelSession.Settings.LicenseAccepted = true;

                    if (isStreamer && ChannelSession.Settings.Channel != null && ChannelSession.MixerStreamerUser.id != ChannelSession.Settings.Channel.userId)
                    {
                        GlobalEvents.ShowMessageBox("The account you are logged in as on Mixer does not match the account for this settings. Please log in as the correct account on Mixer.");
                        ChannelSession.Settings.OAuthToken.accessToken = string.Empty;
                        ChannelSession.Settings.OAuthToken.refreshToken = string.Empty;
                        ChannelSession.Settings.OAuthToken.expiresIn = 0;
                        return false;
                    }

                    ChannelSession.Settings.Channel = channel;

                    ChannelSession.Services.Telemetry.SetUserId(ChannelSession.Settings.TelemetryUserId);

                    ChannelSession.MixerStreamerConnection.Initialize();
                    await MixerChatEmoteModel.InitializeEmoteCache();

                    if (ChannelSession.IsStreamer)
                    {
                        ChannelSession.PreMadeChatCommands.Clear();
                        foreach (PreMadeChatCommand command in ReflectionHelper.CreateInstancesOfImplementingType<PreMadeChatCommand>())
                        {
#pragma warning disable CS0612 // Type or member is obsolete
                            if (!(command is ObsoletePreMadeCommand))
                            {
                                ChannelSession.PreMadeChatCommands.Add(command);
                            }
#pragma warning restore CS0612 // Type or member is obsolete
                        }

                        foreach (PreMadeChatCommandSettings commandSetting in ChannelSession.Settings.PreMadeChatCommandSettings)
                        {
                            PreMadeChatCommand command = ChannelSession.PreMadeChatCommands.FirstOrDefault(c => c.Name.Equals(commandSetting.Name));
                            if (command != null)
                            {
                                command.UpdateFromSettings(commandSetting);
                            }
                        }
                    }

                    MixerChatService mixerChatService = new MixerChatService();

                    if (!await mixerChatService.ConnectStreamer() || !await ChannelSession.Constellation.Connect())
                    {
                        return false;
                    }

                    await ChannelSession.Services.Chat.Initialize(mixerChatService);

                    Dictionary<IExternalService, Task<ExternalServiceResult>> externalServiceTasks = new Dictionary<IExternalService, Task<ExternalServiceResult>>();
                    if (ChannelSession.Settings.StreamlabsOAuthToken != null)
                    {
                        externalServiceTasks[ChannelSession.Services.Streamlabs] = ChannelSession.Services.Streamlabs.Connect(ChannelSession.Settings.StreamlabsOAuthToken);
                    }
                    if (ChannelSession.Settings.StreamJarOAuthToken != null)
                    {
                        externalServiceTasks[ChannelSession.Services.StreamJar] = ChannelSession.Services.StreamJar.Connect(ChannelSession.Settings.StreamJarOAuthToken);
                    }
                    if (ChannelSession.Settings.TipeeeStreamOAuthToken != null)
                    {
                        externalServiceTasks[ChannelSession.Services.TipeeeStream] = ChannelSession.Services.TipeeeStream.Connect(ChannelSession.Settings.TipeeeStreamOAuthToken);
                    }
                    if (ChannelSession.Settings.TreatStreamOAuthToken != null)
                    {
                        externalServiceTasks[ChannelSession.Services.TreatStream] = ChannelSession.Services.TreatStream.Connect(ChannelSession.Settings.TreatStreamOAuthToken);
                    }
                    if (ChannelSession.Settings.StreamlootsOAuthToken != null)
                    {
                        externalServiceTasks[ChannelSession.Services.Streamloots] = ChannelSession.Services.Streamloots.Connect(ChannelSession.Settings.StreamlootsOAuthToken);
                    }

                    if (externalServiceTasks.Count > 0)
                    {
                        await Task.WhenAll(externalServiceTasks.Values);

                        List<IExternalService> failedServices = new List<IExternalService>();
                        foreach (var kvp in externalServiceTasks)
                        {
                            if (!kvp.Value.Result.Success)
                            {
                                ExternalServiceResult result = await kvp.Key.Connect();
                                if (!result.Success)
                                {
                                    failedServices.Add(kvp.Key);
                                }
                            }
                        }

                        if (failedServices.Count > 0)
                        {
                            StringBuilder message = new StringBuilder();
                            message.AppendLine("The following services could not be connected:");
                            message.AppendLine();
                            foreach (IExternalService service in failedServices)
                            {
                                message.AppendLine(" - " + service.Name);
                            }
                            message.AppendLine();
                            message.Append("Please go to the Services page to reconnect them manually.");
                            await DialogHelper.ShowMessage(message.ToString());
                        }
                    }


                    if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
                    {
                        await ChannelSession.Services.InitializeOBSWebsocket();
                    }
                    if (ChannelSession.Settings.EnableStreamlabsOBSConnection)
                    {
                        await ChannelSession.Services.InitializeStreamlabsOBSService();
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

                    if (ChannelSession.Settings.TwitterOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeTwitter();
                    }
                    if (ChannelSession.Settings.DiscordOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeDiscord();
                    }
                    if (ChannelSession.Settings.TiltifyOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeTiltify();
                    }

                    if (ChannelSession.Settings.PatreonOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializePatreon();
                    }
                    if (!string.IsNullOrEmpty(ChannelSession.Settings.OvrStreamServerIP))
                    {
                        await ChannelSession.Services.InitializeOvrStream();
                    }
                    if (ChannelSession.Settings.IFTTTOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeIFTTT();
                    }
                    if (ChannelSession.Settings.ExtraLifeTeamID > 0)
                    {
                        await ChannelSession.Services.InitializeExtraLife();
                    }
                    if (ChannelSession.Settings.JustGivingOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeJustGiving();
                    }

                    if (ChannelSession.Settings.RemoteHostConnection != null)
                    {
                        await ChannelSession.Services.RemoteService.InitializeConnection(ChannelSession.Settings.RemoteHostConnection);
                    }

                    foreach (CommandBase command in ChannelSession.AllEnabledCommands)
                    {
                        foreach (ActionBase action in command.Actions)
                        {
                            if (action is CounterAction)
                            {
                                await ((CounterAction)action).SetCounterValue();
                            }
                        }
                    }

                    if (ChannelSession.Settings.DefaultMixPlayGame > 0)
                    {
                        IEnumerable<MixPlayGameListingModel> games = await ChannelSession.MixerStreamerConnection.GetOwnedMixPlayGames(ChannelSession.MixerChannel);
                        MixPlayGameListingModel game = games.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultMixPlayGame));
                        if (game != null)
                        {
                            if (await ChannelSession.Interactive.Connect(game) != MixPlayConnectionResult.Success)
                            {
                                await ChannelSession.Interactive.Disconnect();
                            }
                        }
                        else
                        {
                            ChannelSession.Settings.DefaultMixPlayGame = 0;
                        }
                    }

                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.ShouldBeReset())
                        {
                            await currency.Reset();
                        }
                    }

                    if (ChannelSession.Settings.ModerationResetStrikesOnLaunch)
                    {
                        foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values.Where(u => u.ModerationStrikes > 0))
                        {
                            userData.ModerationStrikes = 0;
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                    }

                    ChannelSession.Services.TimerService.Initialize();

                    ChannelSession.Services.InputService.HotKeyPressed += InputService_HotKeyPressed;

                    await ChannelSession.SaveSettings();

                    await ChannelSession.Services.Settings.SaveBackup(ChannelSession.Settings);

                    await ChannelSession.Services.Settings.PerformBackupIfApplicable(ChannelSession.Settings);

                    ChannelSession.Services.Telemetry.TrackLogin(ChannelSession.MixerStreamerUser.id.ToString(), ChannelSession.IsStreamer, ChannelSession.MixerChannel.partnered);
                    if (ChannelSession.Settings.IsStreamer)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => { await ChannelSession.Services.MixItUpService.SendUserFeatureEvent(new UserFeatureEvent(ChannelSession.MixerStreamerUser.id)); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }

                    GlobalEvents.OnRankChanged += GlobalEvents_OnRankChanged;

                    AsyncRunner.RunBackgroundTask(sessionBackgroundCancellationTokenSource.Token, 60000, SessionBackgroundTask);

                    return true;
                }
            }
            return false;
        }

        private static async Task<bool> InitializeBotInternal()
        {
            PrivatePopulatedUserModel user = await ChannelSession.MixerBotConnection.GetCurrentUser();
            if (user != null)
            {
                ChannelSession.MixerBotUser = user;

                ChannelSession.MixerBotConnection.Initialize();

                await ChannelSession.Services.Chat.MixerChatService.ConnectBot();

                await ChannelSession.SaveSettings();

                return true;
            }
            return false;
        }

        private static async Task SessionBackgroundTask(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                sessionBackgroundTimer++;

                await ChannelSession.RefreshUser();

                await ChannelSession.RefreshChannel();

                if (sessionBackgroundTimer >= 5)
                {
                    await ChannelSession.SaveSettings();
                    sessionBackgroundTimer = 0;
                }
            }
        }

        private static async void GlobalEvents_OnRankChanged(object sender, UserCurrencyDataViewModel currency)
        {
            if (currency.Currency.RankChangedCommand != null)
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByID(currency.User.ID);
                if (user != null)
                {
                    await currency.Currency.RankChangedCommand.Perform(user);
                }
            }
        }

        private static async void InputService_HotKeyPressed(object sender, HotKey hotKey)
        {
            if (ChannelSession.Settings.HotKeys.ContainsKey(hotKey.ToString()))
            {
                HotKeyConfiguration hotKeyConfiguration = ChannelSession.Settings.HotKeys[hotKey.ToString()];
                CommandBase command = ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(hotKeyConfiguration.CommandID));
                if (command != null)
                {
                    await command.Perform();
                }
            }
        }
    }
}