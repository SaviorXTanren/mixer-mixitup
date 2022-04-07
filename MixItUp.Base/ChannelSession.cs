using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
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
using Twitch.Base.Models.NewAPI.Channels;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static ApplicationSettingsV2Model AppSettings { get; private set; }
        public static SettingsV3Model Settings { get; private set; }
        public static UserV2ViewModel User { get; private set; }

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

        public static async Task Initialize()
        {
            ServiceManager.Add(new SecretsService());

            ServiceManager.Add(new CommandService());
            ServiceManager.Add(new SettingsService());
            ServiceManager.Add(new MixItUpService());
            ServiceManager.Add(new UserService());
            ServiceManager.Add(new ChatService());
            ServiceManager.Add(new EventService());
            ServiceManager.Add(new AlertsService());
            ServiceManager.Add(new StatisticsService());
            ServiceManager.Add(new ModerationService());
            ServiceManager.Add(new TimerService());
            ServiceManager.Add(new GameQueueService());
            ServiceManager.Add(new GiveawayService());
            ServiceManager.Add(new SerialService());
            ServiceManager.Add(new OverlayService());

            ServiceManager.Add(new StreamlabsOBSService());
            ServiceManager.Add(new XSplitService());
            ServiceManager.Add(new PolyPopService());

            ServiceManager.Add(new StreamlootsService());
            ServiceManager.Add(new JustGivingService());
            ServiceManager.Add(new TiltifyService());
            ServiceManager.Add(new ExtraLifeService());
            ServiceManager.Add(new IFTTTService());
            ServiceManager.Add(new PatreonService());
            ServiceManager.Add(new DiscordService());
            ServiceManager.Add(new TwitterService());
            ServiceManager.Add(new PixelChatService());
            ServiceManager.Add(new VTubeStudioService());
            try
            {
                Type voicemodServiceType = Type.GetType("MixItUp.Base.Services.External.VoicemodService");
                if (voicemodServiceType != null)
                {
                    ServiceManager.Add((IVoicemodService)Activator.CreateInstance(voicemodServiceType));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            ServiceManager.Add(new TwitchSessionService());
            ServiceManager.Add(new TwitchChatService());
            ServiceManager.Add(new TwitchEventService());
            ServiceManager.Add(new TwitchStatusService());

            ServiceManager.Add(new YouTubeSessionService());
            ServiceManager.Add(new YouTubeChatService());

            ServiceManager.Add(new GlimeshSessionService());
            ServiceManager.Add(new GlimeshChatEventService());

            ServiceManager.Add(new TrovoSessionService());
            ServiceManager.Add(new TrovoChatEventService());

            try
            {
                Type mixItUpSecretsType = Type.GetType("MixItUp.Base.MixItUpSecrets");
                if (mixItUpSecretsType != null)
                {
                    ServiceManager.Add((SecretsService)Activator.CreateInstance(mixItUpSecretsType));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            ServiceManager.Get<SettingsService>().Initialize();

            ChannelSession.AppSettings = await ApplicationSettingsV2Model.Load();
        }

        public static async Task Close()
        {
            if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ApplicationExit, new CommandParametersModel()))
            {
                // Adding artificial 3 second delay to allow application exit command to finish executing
                await Task.Delay(3000);
            }

            foreach (IExternalService service in ServiceManager.GetAll<IExternalService>())
            {
                await service.Disconnect();
            }

            if (ChannelSession.Settings != null)
            {
                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    if (ChannelSession.Settings.StreamingPlatformAuthentications.ContainsKey(p) && StreamingPlatforms.GetPlatformSessionService(p).IsConnected)
                    {
                        await StreamingPlatforms.GetPlatformSessionService(p).CloseUser();
                        await StreamingPlatforms.GetPlatformSessionService(p).CloseBot();
                    }
                });
            }
        }

        public static async Task SaveSettings()
        {
            await ServiceManager.Get<SettingsService>().Save(ChannelSession.Settings);
        }

        public static async Task<Result> Connect(SettingsV3Model settings)
        {
            Result result = new Result();

            await StreamingPlatforms.ForEachPlatform(async (p) =>
            {
                if (settings.StreamingPlatformAuthentications.ContainsKey(p) && settings.StreamingPlatformAuthentications[p].IsEnabled)
                {
                    result.Combine(await StreamingPlatforms.GetPlatformSessionService(p).Connect(settings));
                }
            });

            if (result.Success)
            {
                ChannelSession.Settings = settings;
            }
            return result;
        }

        public static async Task<Result> InitializeSession()
        {
            if (ChannelSession.Settings == null)
            {
                return new Result(MixItUp.Base.Resources.SettingsNoFileHasBeenLoaded);
            }

            try
            {
                await ServiceManager.Get<SettingsService>().Initialize(ChannelSession.Settings);

                await ServiceManager.Get<ITelemetryService>().Connect();
                ServiceManager.Get<ITelemetryService>().SetUserID(ChannelSession.Settings.TelemetryUserID);

                Result result = new Result();
                foreach (IStreamingPlatformSessionService streamingPlatformSessionService in ServiceManager.GetAll<IStreamingPlatformSessionService>())
                {
                    if (streamingPlatformSessionService.IsConnected)
                    {
                        result.Combine(await streamingPlatformSessionService.InitializeUser(ChannelSession.Settings));
                        result.Combine(await streamingPlatformSessionService.InitializeBot(ChannelSession.Settings));
                    }
                }

                if (!result.Success)
                {
                    return result;
                }

                foreach (IStreamingPlatformSessionService streamingPlatformSessionService in ServiceManager.GetAll<IStreamingPlatformSessionService>())
                {
                    if (streamingPlatformSessionService.IsConnected)
                    {
                        streamingPlatformSessionService.SaveSettings(ChannelSession.Settings);
                    }
                }

                StreamingPlatforms.ForEachPlatform(p =>
                {
                    if (ChannelSession.Settings.StreamingPlatformAuthentications.ContainsKey(p) && StreamingPlatforms.GetPlatformSessionService(p).IsConnected)
                    {
                        StreamingPlatforms.GetPlatformSessionService(p).SaveSettings(ChannelSession.Settings);
                    }
                });

                foreach (SettingsV3Model setting in await ServiceManager.Get<SettingsService>().GetAllSettings())
                {
                    if (ChannelSession.Settings.ID != setting.ID)
                    {
                        StreamingPlatforms.ForEachPlatform(p =>
                        {
                            if (setting.StreamingPlatformAuthentications.ContainsKey(p) && ChannelSession.Settings.StreamingPlatformAuthentications.ContainsKey(p))
                            {
                                if (setting.StreamingPlatformAuthentications[p].IsEnabled && setting.StreamingPlatformAuthentications[p].Equals(ChannelSession.Settings.StreamingPlatformAuthentications[p]))
                                {
                                    result = new Result(string.Format(MixItUp.Base.Resources.SettingsAlreadyExistForAccount, p));
                                    return;
                                }
                            }
                        });

                        if (!result.Success)
                        {
                            return result;
                        }
                    }
                }

                if (StreamingPlatforms.GetPlatformSessionService(ChannelSession.Settings.DefaultStreamingPlatform).IsConnected)
                {
                    ChannelSession.Settings.Name = StreamingPlatforms.GetPlatformSessionService(ChannelSession.Settings.DefaultStreamingPlatform).Username;
                }
                else if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    ChannelSession.Settings.Name = ServiceManager.Get<TwitchSessionService>().Username;
                }
                else if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
                {
                    ChannelSession.Settings.Name = ServiceManager.Get<YouTubeSessionService>().Username;
                }
                else if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
                {
                    ChannelSession.Settings.Name = ServiceManager.Get<GlimeshSessionService>().Username;
                }
                else if (ServiceManager.Get<TrovoSessionService>().IsConnected)
                {
                    ChannelSession.Settings.Name = ServiceManager.Get<TrovoSessionService>().Username;
                }
                else
                {
                    ChannelSession.Settings.Name = "Test";
                }

                if (StreamingPlatforms.GetPlatformSessionService(ChannelSession.Settings.DefaultStreamingPlatform).IsConnected)
                {
                    ChannelSession.User = await ServiceManager.Get<UserService>().GetUserByPlatformID(ChannelSession.Settings.DefaultStreamingPlatform, StreamingPlatforms.GetPlatformSessionService(ChannelSession.Settings.DefaultStreamingPlatform).UserID);
                }
                if (ChannelSession.User == null && ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    ChannelSession.User = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, ServiceManager.Get<TwitchSessionService>().UserID);
                    if (ChannelSession.User == null)
                    {
                        ChannelSession.User = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(ServiceManager.Get<TwitchSessionService>().User));
                    }
                }
                if (ChannelSession.User == null && ServiceManager.Get<YouTubeSessionService>().IsConnected)
                {
                    ChannelSession.User = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.YouTube, ServiceManager.Get<YouTubeSessionService>().UserID);
                    if (ChannelSession.User == null)
                    {
                        ChannelSession.User = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(ServiceManager.Get<YouTubeSessionService>().User));
                    }
                }
                if (ChannelSession.User == null && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                {
                    ChannelSession.User = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, ServiceManager.Get<GlimeshSessionService>().UserID);
                    if (ChannelSession.User == null)
                    {
                        ChannelSession.User = await ServiceManager.Get<UserService>().CreateUser(new GlimeshUserPlatformV2Model(ServiceManager.Get<GlimeshSessionService>().User));
                    }
                }
                if (ChannelSession.User == null && ServiceManager.Get<TrovoSessionService>().IsConnected)
                {
                    ChannelSession.User = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Trovo, ServiceManager.Get<TrovoSessionService>().UserID);
                    if (ChannelSession.User == null)
                    {
                        ChannelSession.User = await ServiceManager.Get<UserService>().CreateUser(new TrovoUserPlatformV2Model(ServiceManager.Get<TrovoSessionService>().User));
                    }
                }

                if (ChannelSession.User == null)
                {
                    return new Result(MixItUp.Base.Resources.InitializeSessionUserInitializationFailed);
                }
                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(ChannelSession.User);

                await ServiceManager.Get<ChatService>().Initialize();
                await ServiceManager.Get<EventService>().Initialize();

                // Connect External Services
                Dictionary<IExternalService, OAuthTokenModel> externalServiceToConnect = new Dictionary<IExternalService, OAuthTokenModel>();
                if (ChannelSession.Settings.StreamlabsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamlabsService>()] = ChannelSession.Settings.StreamlabsOAuthToken; }
                if (ChannelSession.Settings.StreamElementsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamElementsService>()] = ChannelSession.Settings.StreamElementsOAuthToken; }
                if (ChannelSession.Settings.RainMakerOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<RainmakerService>()] = ChannelSession.Settings.RainMakerOAuthToken; }
                if (ChannelSession.Settings.TipeeeStreamOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TipeeeStreamService>()] = ChannelSession.Settings.TipeeeStreamOAuthToken; }
                if (ChannelSession.Settings.TreatStreamOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TreatStreamService>()] = ChannelSession.Settings.TreatStreamOAuthToken; }
                if (ChannelSession.Settings.StreamlootsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamlootsService>()] = ChannelSession.Settings.StreamlootsOAuthToken; }
                if (ChannelSession.Settings.TiltifyOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TiltifyService>()] = ChannelSession.Settings.TiltifyOAuthToken; }
                if (ChannelSession.Settings.JustGivingOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<JustGivingService>()] = ChannelSession.Settings.JustGivingOAuthToken; }
                if (ChannelSession.Settings.IFTTTOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<IFTTTService>()] = ChannelSession.Settings.IFTTTOAuthToken; }
                if (ChannelSession.Settings.ExtraLifeTeamID > 0) { externalServiceToConnect[ServiceManager.Get<ExtraLifeService>()] = new OAuthTokenModel(); }
                if (ChannelSession.Settings.PatreonOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<PatreonService>()] = ChannelSession.Settings.PatreonOAuthToken; }
                if (ChannelSession.Settings.DiscordOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<DiscordService>()] = ChannelSession.Settings.DiscordOAuthToken; }
                if (ChannelSession.Settings.TwitterOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TwitterService>()] = ChannelSession.Settings.TwitterOAuthToken; }
                if (ChannelSession.Settings.PixelChatOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<PixelChatService>()] = ChannelSession.Settings.PixelChatOAuthToken; }
                if (ChannelSession.Settings.VTubeStudioOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<VTubeStudioService>()] = ChannelSession.Settings.VTubeStudioOAuthToken; }
                if (ChannelSession.Settings.EnableVoicemodStudio) { externalServiceToConnect[ServiceManager.Get<IVoicemodService>()] = null; }
                if (ServiceManager.Get<IOBSStudioService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<IOBSStudioService>()] = null; }
                if (ServiceManager.Get<StreamlabsOBSService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<StreamlabsOBSService>()] = null; }
                if (ServiceManager.Get<XSplitService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<XSplitService>()] = null; }
                if (!string.IsNullOrEmpty(ChannelSession.Settings.OvrStreamServerIP)) { externalServiceToConnect[ServiceManager.Get<IOvrStreamService>()] = null; }
                if (ChannelSession.Settings.PolyPopPortNumber > 0) { externalServiceToConnect[ServiceManager.Get<PolyPopService>()] = null; }
                if (ChannelSession.Settings.EnableOverlay) { externalServiceToConnect[ServiceManager.Get<OverlayService>()] = null; }
                if (ChannelSession.Settings.EnableDeveloperAPI) { externalServiceToConnect[ServiceManager.Get<IDeveloperAPIService>()] = null; }

                if (externalServiceToConnect.Count > 0)
                {
                    Dictionary<IExternalService, Task<Result>> externalServiceTasks = new Dictionary<IExternalService, Task<Result>>();
                    foreach (var kvp in externalServiceToConnect)
                    {
                        Logger.Log(LogLevel.Debug, "Trying automatic OAuth service connection: " + kvp.Key.GetType().ToString());

                        try
                        {
                            if (kvp.Key is IOAuthExternalService && kvp.Value != null)
                            {
                                externalServiceTasks[kvp.Key] = ((IOAuthExternalService)kvp.Key).Connect(kvp.Value);
                            }
                            else
                            {
                                externalServiceTasks[kvp.Key] = kvp.Key.Connect();
                            }
                        }
                        catch (Exception sex)
                        {
                            Logger.Log(LogLevel.Error, "Error in external service initial connection: " + kvp.Key.GetType().ToString());
                            Logger.Log(sex);
                        }
                    }

                    try
                    {
                        await Task.WhenAll(externalServiceTasks.Values);
                    }
                    catch (Exception sex)
                    {
                        Logger.Log(LogLevel.Error, "Error in batch external service connection");
                        Logger.Log(sex);
                    }

                    List<IExternalService> failedServices = new List<IExternalService>();
                    foreach (var kvp in externalServiceTasks)
                    {
                        try
                        {
                            if (kvp.Value.Result != null && !kvp.Value.Result.Success && kvp.Key is IOAuthExternalService)
                            {
                                Logger.Log(LogLevel.Debug, "Automatic OAuth token connection failed, trying manual connection: " + kvp.Key.GetType().ToString());
                                result = await kvp.Key.Connect();
                                if (!result.Success)
                                {
                                    failedServices.Add(kvp.Key);
                                }
                            }
                        }
                        catch (Exception sex)
                        {
                            Logger.Log(LogLevel.Error, "Error in external service failed re-connection: " + kvp.Key.GetType().ToString());
                            Logger.Log(sex);
                            failedServices.Add(kvp.Key);
                        }
                    }

                    if (failedServices.Count > 0)
                    {
                        Logger.Log(LogLevel.Debug, "Connection failed for services: " + string.Join(", ", failedServices.Select(s => s.GetType().ToString())));

                        StringBuilder failedServiceMessage = new StringBuilder();
                        foreach (IExternalService service in failedServices)
                        {
                            failedServiceMessage.AppendLine(" - " + service.Name);
                        }
                        await DialogHelper.ShowMessage(string.Format(MixItUp.Base.Resources.ConnectedServicesFailed, failedServiceMessage.ToString()));
                    }
                }

                try
                {
                    ServiceManager.Get<MixItUpService>().BackgroundConnect();

                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        if (currency.ShouldBeReset())
                        {
                            await currency.Reset();
                        }
                    }

                    await ServiceManager.Get<CommandService>().Initialize();
                    await ServiceManager.Get<TimerService>().Initialize();
                    await ServiceManager.Get<ModerationService>().Initialize();

                    if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        IEnumerable<ChannelEditorUserModel> channelEditors = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetChannelEditors(ServiceManager.Get<TwitchSessionService>().User);
                        if (channelEditors != null)
                        {
                            foreach (ChannelEditorUserModel channelEditor in channelEditors)
                            {
                                ServiceManager.Get<TwitchSessionService>().ChannelEditors.Add(channelEditor.user_id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    Logger.Log(LogLevel.Error, "Streamer Services - " + JSONSerializerHelper.SerializeToString(ex));
                    await DialogHelper.ShowMessage(Resources.FailedToInitializeStreamerBasedServices +
                        Environment.NewLine + Environment.NewLine + Resources.ErrorDetailsHeader + " " + ex.Message);
                    return new Result(ex.Message);
                }

                await ServiceManager.Get<TimerService>().Initialize();
                await ServiceManager.Get<ModerationService>().Initialize();
                ServiceManager.Get<StatisticsService>().Initialize();

                ServiceManager.Get<IInputService>().HotKeyPressed += InputService_HotKeyPressed;

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

                await ChannelSession.SaveSettings();
                await ServiceManager.Get<SettingsService>().SaveLocalBackup(ChannelSession.Settings);
                await ServiceManager.Get<SettingsService>().PerformAutomaticBackupIfApplicable(ChannelSession.Settings);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(SessionBackgroundTask, sessionBackgroundCancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                ServiceManager.Get<ITelemetryService>().TrackLogin(ChannelSession.Settings.TelemetryUserID, ServiceManager.Get<TwitchSessionService>().User?.broadcaster_type);

                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ApplicationLaunch, new CommandParametersModel());

                return new Result();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log(LogLevel.Error, "Session Initialization - " + JSONSerializerHelper.SerializeToString(ex));
                return new Result(MixItUp.Base.Resources.FailedToInitializeSession + Environment.NewLine + Environment.NewLine + MixItUp.Base.Resources.ErrorHeader + ex.Message);
            }
        }

        public static void DisconnectionOccurred(string serviceName)
        {
            Logger.Log(LogLevel.Error, serviceName + " Service disconnection occurred");
            GlobalEvents.ServiceDisconnect(serviceName);
        }

        public static void ReconnectionOccurred(string serviceName)
        {
            Logger.Log(LogLevel.Error, serviceName + " Service reconnection successful");
            GlobalEvents.ServiceReconnect(serviceName);
        }

        private static async Task SessionBackgroundTask(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                sessionBackgroundTimer++;

                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    if (ChannelSession.Settings.StreamingPlatformAuthentications.ContainsKey(p) && StreamingPlatforms.GetPlatformSessionService(p).IsConnected)
                    {
                        await StreamingPlatforms.GetPlatformSessionService(p).RefreshUser();
                        await StreamingPlatforms.GetPlatformSessionService(p).RefreshChannel();
                    }
                });

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
                await ServiceManager.Get<CommandService>().Queue(hotKeyConfiguration.CommandID, new CommandParametersModel(platform: StreamingPlatformTypeEnum.All));
            }
        }
    }
}