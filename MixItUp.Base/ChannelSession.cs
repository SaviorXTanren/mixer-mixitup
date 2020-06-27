using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;
using TwitchV5API = Twitch.Base.Models.V5;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static TwitchPlatformService TwitchUserConnection { get; private set; }
        public static TwitchPlatformService TwitchBotConnection { get; private set; }
        public static TwitchV5API.Users.UserModel TwitchUserV5 { get; private set; }
        public static TwitchV5API.Channel.ChannelModel TwitchChannelV5 { get; private set; }
        public static TwitchV5API.Streams.StreamModel TwitchStreamV5 { get; private set; }
        public static TwitchNewAPI.Users.UserModel TwitchUserNewAPI { get; set; }
        public static TwitchNewAPI.Users.UserModel TwitchBotNewAPI { get; set; }
        public static TwitchNewAPI.Users.UserModel TwitchChannelNewAPI { get; private set; }

        public static bool TwitchStreamIsLive { get { return ChannelSession.TwitchStreamV5 != null && ChannelSession.TwitchStreamV5.IsLive; } }

        public static ApplicationSettingsV2Model AppSettings { get; private set; }
        public static SettingsV2Model Settings { get; private set; }

        public static ServicesManagerBase Services { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }

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

        public static bool IsElevated { get; set; }

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
                commands.AddRange(ChannelSession.Settings.TimerCommands);
                commands.AddRange(ChannelSession.Settings.ActionGroupCommands);
                commands.AddRange(ChannelSession.Settings.TwitchChannelPointsCommands);
                return commands;
            }
        }

        public static bool IsStreamer { get { return ChannelSession.Settings.IsStreamer; } }

        public static async Task Initialize(ServicesManagerBase serviceHandler)
        {
            ChannelSession.Services = serviceHandler;

            try
            {
                Type mixItUpSecretsType = Type.GetType("MixItUp.Base.MixItUpSecrets");
                if (mixItUpSecretsType != null)
                {
                    ChannelSession.Services.SetSecrets((SecretsService)Activator.CreateInstance(mixItUpSecretsType));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            ChannelSession.PreMadeChatCommands = new List<PreMadeChatCommand>();

            ChannelSession.AppSettings = await ApplicationSettingsV2Model.Load();
        }

        public static async Task<Result> ConnectTwitchUser(bool isStreamer)
        {
            Result<TwitchPlatformService> result = await TwitchPlatformService.ConnectUser(isStreamer);
            if (result.Success)
            {
                ChannelSession.TwitchUserConnection = result.Value;
                ChannelSession.TwitchUserNewAPI = await ChannelSession.TwitchUserConnection.GetNewAPICurrentUser();
                if (ChannelSession.TwitchUserNewAPI == null)
                {
                    return new Result("Failed to get New API Twitch user data");
                }

                ChannelSession.TwitchUserV5 = await ChannelSession.TwitchUserConnection.GetV5APIUserByLogin(ChannelSession.TwitchUserNewAPI.login);
                if (ChannelSession.TwitchUserV5 == null)
                {
                    return new Result("Failed to get V5 API Twitch user data");
                }
            }
            return result;
        }

        public static async Task<Result> ConnectTwitchBot()
        {
            Result<TwitchPlatformService> result = await TwitchPlatformService.ConnectBot();
            if (result.Success)
            {
                ChannelSession.TwitchBotConnection = result.Value;
                ChannelSession.TwitchBotNewAPI = await ChannelSession.TwitchBotConnection.GetNewAPICurrentUser();
                if (ChannelSession.TwitchBotNewAPI == null)
                {
                    return new Result("Failed to get Twitch bot data");
                }

                if (ChannelSession.Services.Chat.TwitchChatService != null && ChannelSession.Services.Chat.TwitchChatService.IsUserConnected)
                {
                    return await ChannelSession.Services.Chat.TwitchChatService.ConnectBot();
                }
            }
            return result;
        }

        public static async Task<Result> ConnectUser(SettingsV2Model settings)
        {
            Result userResult = null;
            ChannelSession.Settings = settings;

            // Twitch connection

            Result<TwitchPlatformService> twitchResult = await TwitchPlatformService.Connect(ChannelSession.Settings.TwitchUserOAuthToken);
            if (twitchResult.Success)
            {
                ChannelSession.TwitchUserConnection = twitchResult.Value;
                userResult = twitchResult;
            }
            else
            {
                userResult = await ChannelSession.ConnectTwitchUser(ChannelSession.Settings.IsStreamer);
            }

            if (userResult.Success)
            {
                ChannelSession.TwitchUserNewAPI = await ChannelSession.TwitchUserConnection.GetNewAPICurrentUser();
                if (ChannelSession.TwitchUserNewAPI == null)
                {
                    return new Result("Failed to get Twitch user data");
                }

                ChannelSession.TwitchUserV5 = await ChannelSession.TwitchUserConnection.GetV5APIUserByLogin(ChannelSession.TwitchUserNewAPI.login);
                if (ChannelSession.TwitchUserV5 == null)
                {
                    return new Result("Failed to get V5 API Twitch user data");
                }

                if (settings.TwitchBotOAuthToken != null)
                {
                    twitchResult = await TwitchPlatformService.Connect(settings.TwitchBotOAuthToken);
                    if (twitchResult.Success)
                    {
                        ChannelSession.TwitchBotConnection = twitchResult.Value;
                        ChannelSession.TwitchBotNewAPI = await ChannelSession.TwitchBotConnection.GetNewAPICurrentUser();
                        if (ChannelSession.TwitchBotNewAPI == null)
                        {
                            return new Result("Failed to get Twitch bot data");
                        }
                    }
                    else
                    {
                        settings.TwitchBotOAuthToken = null;
                        return new Result(success: true, message: "Failed to connect Twitch bot account, please manually reconnect");
                    }
                }
            }
            else
            {
                ChannelSession.Settings.TwitchUserOAuthToken = null;
                return userResult;
            }

            return userResult;
        }

        public static async Task DisconnectTwitchBot()
        {
            ChannelSession.TwitchBotConnection = null;
            if (ChannelSession.Services.Chat.TwitchChatService != null)
            {
                await ChannelSession.Services.Chat.TwitchChatService.DisconnectBot();
            }
        }

        public static async Task Close()
        {
            await ChannelSession.Services.Close();

            if (ChannelSession.Services.Chat.TwitchChatService != null)
            {
                await ChannelSession.Services.Chat.TwitchChatService.DisconnectUser();
            }
            await ChannelSession.DisconnectTwitchBot();
        }

        public static async Task SaveSettings()
        {
            await ChannelSession.Services.Settings.Save(ChannelSession.Settings);
        }

        public static async Task RefreshUser()
        {
            if (ChannelSession.TwitchUserNewAPI != null)
            {
                TwitchNewAPI.Users.UserModel twitchUserNewAPI = await ChannelSession.TwitchUserConnection.GetNewAPICurrentUser();
                if (twitchUserNewAPI != null)
                {
                    ChannelSession.TwitchUserNewAPI = twitchUserNewAPI;

                    TwitchV5API.Users.UserModel twitchUserV5 = await ChannelSession.TwitchUserConnection.GetV5APIUserByLogin(ChannelSession.TwitchUserNewAPI.login);
                    if (twitchUserV5 != null)
                    {
                        ChannelSession.TwitchUserV5 = twitchUserV5;
                    }
                }
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.TwitchChannelV5 != null)
            {
                TwitchV5API.Channel.ChannelModel twitchChannel = await ChannelSession.TwitchUserConnection.GetV5APIChannel(ChannelSession.TwitchChannelV5.id);
                if (twitchChannel != null)
                {
                    ChannelSession.TwitchChannelV5 = twitchChannel;
                    ChannelSession.TwitchStreamV5 = await ChannelSession.TwitchUserConnection.GetV5LiveStream(ChannelSession.TwitchChannelV5);
                }
            }

            if (ChannelSession.TwitchChannelNewAPI != null)
            {
                TwitchNewAPI.Users.UserModel twitchChannel = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(ChannelSession.TwitchChannelNewAPI.login);
                if (twitchChannel != null)
                {
                    ChannelSession.TwitchChannelNewAPI = twitchChannel;
                }
            }
        }

        public static UserViewModel GetCurrentUser()
        {
            // TO-DO: Update UserViewModel so that all platform accounts are combined into the same UserViewModel

            UserViewModel user = null;

            if (ChannelSession.TwitchUserNewAPI != null)
            {
                user = ChannelSession.Services.User.GetUserByTwitchID(ChannelSession.TwitchUserNewAPI.id);
                if (user == null)
                {
                    user = new UserViewModel(ChannelSession.TwitchUserNewAPI);
                }
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

        public static async Task<bool> InitializeSession(string modChannelName = null)
        {
            try
            {
                bool isModerator = !string.IsNullOrEmpty(modChannelName);

                TwitchNewAPI.Users.UserModel twitchChannelNew = null;
                TwitchV5API.Channel.ChannelModel twitchChannelv5 = null;
                if (!isModerator)
                {
                    twitchChannelNew = await ChannelSession.TwitchUserConnection.GetNewAPICurrentUser();
                    twitchChannelv5 = await ChannelSession.TwitchUserConnection.GetCurrentV5APIChannel();
                }
                else
                {
                    twitchChannelNew = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(modChannelName);
                    twitchChannelv5 = await ChannelSession.TwitchUserConnection.GetV5APIChannel(ChannelSession.TwitchChannelV5.id);
                }

                if (twitchChannelNew != null && twitchChannelv5 != null)
                {
                    try
                    {
                        if (isModerator && twitchChannelNew.id == ChannelSession.TwitchUserNewAPI.id)
                        {
                            GlobalEvents.ShowMessageBox($"You are trying to sign in as a moderator to your own channel. Please use the Streamer login to access your channel.");
                            return false;
                        }

                        ChannelSession.TwitchChannelNewAPI = twitchChannelNew;
                        ChannelSession.TwitchChannelV5 = twitchChannelv5;

                        if (ChannelSession.Settings == null)
                        {
                            IEnumerable<SettingsV2Model> currentSettings = await ChannelSession.Services.Settings.GetAllSettings();

                            if (currentSettings.Any(s => !string.IsNullOrEmpty(s.TwitchChannelID) && string.Equals(s.TwitchChannelID, twitchChannelNew.id) && s.IsStreamer == !isModerator))
                            {
                                GlobalEvents.ShowMessageBox($"There already exists settings for the account {twitchChannelNew.display_name}. Please sign in with a different account or re-launch Mix It Up to select those settings from the drop-down.");
                                return false;
                            }

                            ChannelSession.Settings = await ChannelSession.Services.Settings.Create(twitchChannelNew.display_name, modChannelName == null);
                        }
                        await ChannelSession.Services.Settings.Initialize(ChannelSession.Settings);

                        if (!string.IsNullOrEmpty(ChannelSession.Settings.TwitchUserID) && !string.Equals(ChannelSession.TwitchUserNewAPI.id, ChannelSession.Settings.TwitchUserID))
                        {
                            Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {ChannelSession.TwitchUserNewAPI.display_name} - {ChannelSession.TwitchUserNewAPI.id} - {ChannelSession.Settings.TwitchUserID}");
                            GlobalEvents.ShowMessageBox("The account you are logged in as on Twitch does not match the account for this settings. Please log in as the correct account on Twitch.");
                            ChannelSession.Settings.TwitchUserOAuthToken.accessToken = string.Empty;
                            ChannelSession.Settings.TwitchUserOAuthToken.refreshToken = string.Empty;
                            ChannelSession.Settings.TwitchUserOAuthToken.expiresIn = 0;
                            return false;
                        }

                        ChannelSession.Settings.Name = ChannelSession.TwitchChannelNewAPI.display_name;

                        ChannelSession.Settings.TwitchUserID = ChannelSession.TwitchUserNewAPI.id;
                        ChannelSession.Settings.TwitchChannelID = ChannelSession.TwitchChannelNewAPI.id;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        Logger.Log(LogLevel.Error, "Initialize Settings - " + JSONSerializerHelper.SerializeToString(ex));
                        await DialogHelper.ShowMessage("Failed to initialize settings. If this continues, please visit the Mix It Up Discord for assistance." +
                            Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                        return false;
                    }

                    try
                    {
                        await ChannelSession.Services.Telemetry.Connect();
                        ChannelSession.Services.Telemetry.SetUserID(ChannelSession.Settings.TelemetryUserID);

                        TwitchChatService twitchChatService = new TwitchChatService();
                        TwitchEventService twitchEventService = new TwitchEventService();

                        List<Task<Result>> twitchPlatformServiceTasks = new List<Task<Result>>();
                        twitchPlatformServiceTasks.Add(twitchChatService.ConnectUser());
                        twitchPlatformServiceTasks.Add(twitchEventService.Connect());

                        if (twitchPlatformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, twitchPlatformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            GlobalEvents.ShowMessageBox("Failed to connect to Twitch services:" + Environment.NewLine + Environment.NewLine + errors);
                            return false;
                        }

                        await ChannelSession.Services.Chat.Initialize(twitchChatService);
                        await ChannelSession.Services.Events.Initialize(twitchEventService);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        Logger.Log(LogLevel.Error, "Twitch Services - " + JSONSerializerHelper.SerializeToString(ex));
                        await DialogHelper.ShowMessage("Failed to connect to Twitch services. If this continues, please visit the Mix It Up Discord for assistance." +
                            Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                        return false;
                    }

                    if (ChannelSession.IsStreamer)
                    {
                        Result result = await ChannelSession.InitializeBotInternal();
                        if (!result.Success)
                        {
                            await DialogHelper.ShowMessage("Failed to initialize Bot account");
                            return false;
                        }

                        try
                        {
                            // Connect External Services
                            Dictionary<IExternalService, OAuthTokenModel> externalServiceToConnect = new Dictionary<IExternalService, OAuthTokenModel>();
                            if (ChannelSession.Settings.StreamlabsOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Streamlabs] = ChannelSession.Settings.StreamlabsOAuthToken; }
                            if (ChannelSession.Settings.StreamElementsOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.StreamElements] = ChannelSession.Settings.StreamElementsOAuthToken; }
                            if (ChannelSession.Settings.StreamJarOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.StreamJar] = ChannelSession.Settings.StreamJarOAuthToken; }
                            if (ChannelSession.Settings.TipeeeStreamOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.TipeeeStream] = ChannelSession.Settings.TipeeeStreamOAuthToken; }
                            if (ChannelSession.Settings.TreatStreamOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.TreatStream] = ChannelSession.Settings.TreatStreamOAuthToken; }
                            if (ChannelSession.Settings.StreamlootsOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Streamloots] = ChannelSession.Settings.StreamlootsOAuthToken; }
                            if (ChannelSession.Settings.TiltifyOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Tiltify] = ChannelSession.Settings.TiltifyOAuthToken; }
                            if (ChannelSession.Settings.JustGivingOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.JustGiving] = ChannelSession.Settings.JustGivingOAuthToken; }
                            if (ChannelSession.Settings.IFTTTOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.IFTTT] = ChannelSession.Settings.IFTTTOAuthToken; }
                            if (ChannelSession.Settings.ExtraLifeTeamID > 0) { externalServiceToConnect[ChannelSession.Services.ExtraLife] = new OAuthTokenModel(); }
                            if (ChannelSession.Settings.PatreonOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Patreon] = ChannelSession.Settings.PatreonOAuthToken; }
                            if (ChannelSession.Settings.DiscordOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Discord] = ChannelSession.Settings.DiscordOAuthToken; }
                            if (ChannelSession.Settings.TwitterOAuthToken != null) { externalServiceToConnect[ChannelSession.Services.Twitter] = ChannelSession.Settings.TwitterOAuthToken; }
                            if (ChannelSession.Services.OBSStudio.IsEnabled) { externalServiceToConnect[ChannelSession.Services.OBSStudio] = null; }
                            if (ChannelSession.Services.StreamlabsOBS.IsEnabled) { externalServiceToConnect[ChannelSession.Services.StreamlabsOBS] = null; }
                            if (ChannelSession.Services.XSplit.IsEnabled) { externalServiceToConnect[ChannelSession.Services.XSplit] = null; }
                            if (!string.IsNullOrEmpty(ChannelSession.Settings.OvrStreamServerIP)) { externalServiceToConnect[ChannelSession.Services.OvrStream] = null; }
                            if (ChannelSession.Settings.EnableOverlay) { externalServiceToConnect[ChannelSession.Services.Overlay] = null; }
                            if (ChannelSession.Settings.EnableDeveloperAPI) { externalServiceToConnect[ChannelSession.Services.DeveloperAPI] = null; }

                            if (externalServiceToConnect.Count > 0)
                            {
                                Dictionary<IExternalService, Task<Result>> externalServiceTasks = new Dictionary<IExternalService, Task<Result>>();
                                foreach (var kvp in externalServiceToConnect)
                                {
                                    Logger.Log(LogLevel.Debug, "Trying automatic OAuth service connection: " + kvp.Key.Name);

                                    if (kvp.Key is IOAuthExternalService && kvp.Value != null)
                                    {
                                        externalServiceTasks[kvp.Key] = ((IOAuthExternalService)kvp.Key).Connect(kvp.Value);
                                    }
                                    else
                                    {
                                        externalServiceTasks[kvp.Key] = kvp.Key.Connect();
                                    }
                                }
                                await Task.WhenAll(externalServiceTasks.Values);

                                List<IExternalService> failedServices = new List<IExternalService>();
                                foreach (var kvp in externalServiceTasks)
                                {
                                    if (!kvp.Value.Result.Success && kvp.Key is IOAuthExternalService)
                                    {
                                        Logger.Log(LogLevel.Debug, "Automatic OAuth token connection failed, trying manual connection: " + kvp.Key.Name);

                                        result = await kvp.Key.Connect();
                                        if (!result.Success)
                                        {
                                            failedServices.Add(kvp.Key);
                                        }
                                    }
                                }

                                if (failedServices.Count > 0)
                                {
                                    Logger.Log(LogLevel.Debug, "Connection failed for services: " + string.Join(", ", failedServices.Select(s => s.Name)));

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
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            Logger.Log(LogLevel.Error, "External Services - " + JSONSerializerHelper.SerializeToString(ex));
                            await DialogHelper.ShowMessage("Failed to initialize external services. If this continues, please visit the Mix It Up Discord for assistance." +
                                Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                            return false;
                        }

                        try
                        {
                            //if (ChannelSession.Settings.RemoteHostConnection != null)
                            //{
                            //    await ChannelSession.Services.RemoteService.InitializeConnection(ChannelSession.Settings.RemoteHostConnection);
                            //}

                            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                            {
                                if (currency.ShouldBeReset())
                                {
                                    await currency.Reset();
                                }
                            }

                            if (ChannelSession.Settings.ModerationResetStrikesOnLaunch)
                            {
                                foreach (UserDataModel userData in ChannelSession.Settings.UserData.Values)
                                {
                                    if (userData.ModerationStrikes > 0)
                                    {
                                        userData.ModerationStrikes = 0;
                                        ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                                    }
                                }
                            }

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
                            ChannelSession.Services.Chat.RebuildCommandTriggers();

                            ChannelSession.Services.TimerService.Initialize();
                            await ChannelSession.Services.Moderation.Initialize();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            Logger.Log(LogLevel.Error, "Streamer Services - " + JSONSerializerHelper.SerializeToString(ex));
                            await DialogHelper.ShowMessage("Failed to initialize streamer-based services. If this continues, please visit the Mix It Up Discord for assistance." +
                                Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                            return false;
                        }
                    }

                    try
                    {
                        ChannelSession.Services.Statistics.Initialize();

                        ChannelSession.Services.InputService.HotKeyPressed += InputService_HotKeyPressed;

                        foreach (RedemptionStoreProductModel product in ChannelSession.Settings.RedemptionStoreProducts.Values)
                        {
                            product.ReplenishAmount();
                        }

                        foreach (RedemptionStorePurchaseModel purchase in ChannelSession.Settings.RedemptionStorePurchases.ToList())
                        {
                            if (purchase.State != RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded)
                            {
                                ChannelSession.Settings.RedemptionStorePurchases.Remove(purchase);
                            }
                        }

                        ChannelSession.Services.Telemetry.TrackLogin(ChannelSession.Settings.TelemetryUserID, ChannelSession.TwitchChannelNewAPI?.broadcaster_type);

                        await ChannelSession.SaveSettings();
                        await ChannelSession.Services.Settings.SaveLocalBackup(ChannelSession.Settings);
                        await ChannelSession.Services.Settings.PerformAutomaticBackupIfApplicable(ChannelSession.Settings);

                        AsyncRunner.RunBackgroundTask(sessionBackgroundCancellationTokenSource.Token, 60000, SessionBackgroundTask);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        Logger.Log(LogLevel.Error, "Finalize Initialization - " + JSONSerializerHelper.SerializeToString(ex));
                        await DialogHelper.ShowMessage("Failed to finalize initialization. If this continues, please visit the Mix It Up Discord for assistance." +
                            Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log(LogLevel.Error, "Channel Information - " + JSONSerializerHelper.SerializeToString(ex));
                await DialogHelper.ShowMessage("Failed to get channel information. If this continues, please visit the Mix It Up Discord for assistance." +
                    Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
            }
            return false;
        }

        private static async Task<Result> InitializeBotInternal()
        {
            if (ChannelSession.TwitchBotConnection != null)
            {
                Result result = await ChannelSession.Services.Chat.TwitchChatService.ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result();
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