using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mock.New;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static event EventHandler OnRestartRequested = delegate { };
        public static void RestartRequested() { OnRestartRequested(null, new EventArgs()); }

        public static ApplicationSettingsV2Model AppSettings { get; private set; }
        public static SettingsV3Model Settings { get; private set; }
        public static UserV2ViewModel User
        {
            get
            {
                StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(ChannelSession.Settings.DefaultStreamingPlatform);
                if (session != null && session.IsConnected)
                {
                    return session.Streamer;
                }

                foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
                {
                    session = StreamingPlatforms.GetPlatformSession(platform);
                    if (session != null && session.IsConnected)
                    {
                        return session.Streamer;
                    }
                }

                return UserV2ViewModel.CreateUnassociated(Resources.Anonymous);
            }
        }

        public static UserV2ViewModel Bot
        {
            get
            {
                StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(ChannelSession.Settings.DefaultStreamingPlatform);
                if (session != null && session.IsConnected)
                {
                    return session.Bot;
                }

                foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
                {
                    session = StreamingPlatforms.GetPlatformSession(platform);
                    if (session != null && session.IsConnected)
                    {
                        return session.Bot;
                    }
                }

                return null;
            }
        }

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

            ServiceManager.Add(new TwitchSession());
            ServiceManager.Add(new YouTubeSession());
            ServiceManager.Add(new TrovoSession());
            ServiceManager.Add(new MockSession());

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
            ServiceManager.Add(new OverlayV3Service());

            ServiceManager.Add(new StreamlabsDesktopService());
            ServiceManager.Add(new XSplitService());
            ServiceManager.Add(new PolyPopService());
            ServiceManager.Add(new AlejoPronounsService());
            ServiceManager.Add(new BetterTTVService());
            ServiceManager.Add(new FrankerFaceZService());
            ServiceManager.Add(new StreamlootsService());
            ServiceManager.Add(new JustGivingService());
            ServiceManager.Add(new TiltifyService());
            ServiceManager.Add(new DonorDriveService());
            ServiceManager.Add(new IFTTTService());
            ServiceManager.Add(new PatreonService());
            ServiceManager.Add(new DiscordService());
            ServiceManager.Add(new PixelChatService());
            ServiceManager.Add(new VTubeStudioService());
            ServiceManager.Add(new CrowdControlService());
            ServiceManager.Add(new SAMMIService());
            ServiceManager.Add(new InfiniteAlbumService());
            ServiceManager.Add(new TITSService());
            ServiceManager.Add(new LumiaStreamService());
            ServiceManager.Add(new PulsoidService());
            ServiceManager.Add(new ResponsiveVoiceService());
            ServiceManager.Add(new VTSPogService());
            ServiceManager.Add(new MtionStudioService());
            ServiceManager.Add(new TikTokTTSService());
            ServiceManager.Add(new MeldStudioService());

            try
            {
                Type voicemodServiceType = Type.GetType("MixItUp.Base.Services.External.VoicemodService");
                if (voicemodServiceType != null) { ServiceManager.Add((IVoicemodService)Activator.CreateInstance(voicemodServiceType)); }

                Type ttsMonsterServiceType = Type.GetType("MixItUp.Base.Services.External.TTSMonsterService");
                if (ttsMonsterServiceType != null) { ServiceManager.Add((ITTSMonsterService)Activator.CreateInstance(ttsMonsterServiceType)); }
            }
            catch (Exception ex) { Logger.Log(ex); }

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
                if (service.IsConnected)
                {
                    await service.Disconnect();
                }
            }

            if (ChannelSession.Settings != null)
            {
                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    if (ChannelSession.Settings.StreamingPlatformAuthentications.ContainsKey(p) && StreamingPlatforms.GetPlatformSession(p).IsConnected)
                    {
                        await StreamingPlatforms.GetPlatformSession(p).DisconnectBot();
                        await StreamingPlatforms.GetPlatformSession(p).DisconnectStreamer();
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
            ChannelSession.Settings = settings;

            Dictionary<StreamingPlatformTypeEnum, Task<Result>> streamerTasks = new Dictionary<StreamingPlatformTypeEnum, Task<Result>>();
            Dictionary<StreamingPlatformTypeEnum, Task<Result>> botTasks = new Dictionary<StreamingPlatformTypeEnum, Task<Result>>();

            StreamingPlatforms.ForEachPlatform((p) =>
            {
                StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(p);
                if (session.IsEnabled)
                {
                    streamerTasks[session.Platform] = session.AutomaticConnectStreamer();
                    if (session.IsBotEnabled)
                    {
                        botTasks[session.Platform] = session.AutomaticConnectBot();
                    }
                }
            });

            await Task.WhenAll(streamerTasks.Values.Concat(botTasks.Values));

            List<string> streamingPlatformsToBeManuallyReconnected = new List<string>();

            foreach (var streamerTask in streamerTasks)
            {
                if (streamerTask.Value.IsCompleted && !streamerTask.Value.Result.Success)
                {
                    StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(streamerTask.Key);

                    Result result = await session.ManualConnectStreamerWithTimeout();
                    if (result != null && !result.Success)
                    {
                        streamingPlatformsToBeManuallyReconnected.Add(EnumLocalizationHelper.GetLocalizedName(session.Platform) + " - " + Resources.StreamerAccount);
                    }
                }
            }

            foreach (var botTask in botTasks)
            {
                if (botTask.Value.IsCompleted && !botTask.Value.Result.Success)
                {
                    streamingPlatformsToBeManuallyReconnected.Add(EnumLocalizationHelper.GetLocalizedName(botTask.Key) + " - " + Resources.BotAccount);

                    StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(botTask.Key);

                    await session.DisableBot();
                }
            }

            if (streamingPlatformsToBeManuallyReconnected.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Resources.TheFollowingStreamingServicesMustBeManuallyReconnected);
                stringBuilder.AppendLine();
                foreach (string service in streamingPlatformsToBeManuallyReconnected)
                {
                    stringBuilder.AppendLine(service);
                }

                await DialogHelper.ShowMessage(stringBuilder.ToString());
            }

            return new Result();
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

                foreach (StreamingPlatformSessionBase streamingPlatformSession in StreamingPlatforms.GetConnectedPlatformSessions())
                {
                    streamingPlatformSession.SaveAuthenticationSettings();
                }

                if (!StreamingPlatforms.SupportedPlatforms.Contains(ChannelSession.Settings.DefaultStreamingPlatform))
                {
                    ChannelSession.Settings.DefaultStreamingPlatform = StreamingPlatforms.GetConnectedPlatforms().FirstOrDefault();
                }

                if (string.IsNullOrEmpty(ChannelSession.Settings.Name))
                {
                    if (StreamingPlatforms.GetPlatformSession(ChannelSession.Settings.DefaultStreamingPlatform).IsConnected)
                    {
                        ChannelSession.Settings.Name = StreamingPlatforms.GetPlatformSession(ChannelSession.Settings.DefaultStreamingPlatform).StreamerUsername;
                    }
                    else
                    {
                        ChannelSession.Settings.Name = "Test";
                    }
                }

                if (ChannelSession.User == null)
                {
                    return new Result(MixItUp.Base.Resources.InitializeSessionUserInitializationFailed);
                }
                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(ChannelSession.User);

                await ServiceManager.Get<ChatService>().Initialize();
                await ServiceManager.Get<EventService>().Initialize();

                Dictionary<ServiceBase, Task<Result>> serviceConnectionTasks = new Dictionary<ServiceBase, Task<Result>>();
                foreach (ServiceBase service in ServiceManager.GetAll<ServiceBase>())
                {
                    if (service.IsEnabled)
                    {
                        serviceConnectionTasks[service] = service.AutomaticConnect();
                    }
                }
                await Task.WhenAll(serviceConnectionTasks.Values);

                List<string> failedServiceNames = new List<string>();
                foreach (var kvp in serviceConnectionTasks)
                {
                    if (!kvp.Value.IsCompleted || !kvp.Value.Result.Success)
                    {
                        if (kvp.Key is OAuthServiceBase)
                        {
                            Result result = await kvp.Key.ManualConnectWithTimeout();
                            if (!result.Success)
                            {
                                failedServiceNames.Add(kvp.Key.Name);
                            }
                        }
                    }
                }

                if (failedServiceNames.Count > 0)
                {
                    StringBuilder failedServiceNamesMessage = new StringBuilder();
                    foreach (string failedServiceName in failedServiceNames)
                    {
                        failedServiceNamesMessage.AppendLine(" - " + failedServiceName);
                    }
                    await DialogHelper.ShowMessage(string.Format(MixItUp.Base.Resources.ConnectedServicesFailed, failedServiceNamesMessage.ToString()));
                }

                // Connect External Services
                Dictionary<IExternalService, OAuthTokenModel> externalServiceToConnect = new Dictionary<IExternalService, OAuthTokenModel>();
                if (ChannelSession.Settings.StreamlabsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamlabsService>()] = ChannelSession.Settings.StreamlabsOAuthToken; }
                if (ChannelSession.Settings.StreamElementsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamElementsService>()] = ChannelSession.Settings.StreamElementsOAuthToken; }
                if (ChannelSession.Settings.RainMakerOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<RainmakerService>()] = ChannelSession.Settings.RainMakerOAuthToken; }
                if (ChannelSession.Settings.TipeeeStreamOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TipeeeStreamService>()] = ChannelSession.Settings.TipeeeStreamOAuthToken; }
                if (ChannelSession.Settings.TreatStreamOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TreatStreamService>()] = ChannelSession.Settings.TreatStreamOAuthToken; }
                if (ChannelSession.Settings.StreamlootsOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<StreamlootsService>()] = ChannelSession.Settings.StreamlootsOAuthToken; }
                if (ChannelSession.Settings.TiltifyOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TiltifyService>()] = ChannelSession.Settings.TiltifyOAuthToken; }
                if (ChannelSession.Settings.JustGivingPageShortName != null) { externalServiceToConnect[ServiceManager.Get<JustGivingService>()] = null; }
                if (ChannelSession.Settings.IFTTTOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<IFTTTService>()] = ChannelSession.Settings.IFTTTOAuthToken; }
                if (!string.IsNullOrEmpty(ChannelSession.Settings.DonorDriveParticipantID)) { externalServiceToConnect[ServiceManager.Get<DonorDriveService>()] = null; }
                if (ChannelSession.Settings.PatreonOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<PatreonService>()] = ChannelSession.Settings.PatreonOAuthToken; }
                if (ChannelSession.Settings.DiscordOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<DiscordService>()] = ChannelSession.Settings.DiscordOAuthToken; }
                if (ChannelSession.Settings.PixelChatOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<PixelChatService>()] = ChannelSession.Settings.PixelChatOAuthToken; }
                if (ChannelSession.Settings.VTubeStudioOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<VTubeStudioService>()] = ChannelSession.Settings.VTubeStudioOAuthToken; }
                if (ChannelSession.Settings.InfiniteAlbumOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<InfiniteAlbumService>()] = ChannelSession.Settings.InfiniteAlbumOAuthToken; }
                if (ChannelSession.Settings.TITSOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<TITSService>()] = ChannelSession.Settings.TITSOAuthToken; }
                if (ChannelSession.Settings.LumiaStreamOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<LumiaStreamService>()] = ChannelSession.Settings.LumiaStreamOAuthToken; }
                if (ChannelSession.Settings.PulsoidOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<PulsoidService>()] = ChannelSession.Settings.PulsoidOAuthToken; }
                if (ChannelSession.Settings.EnableVoicemodStudio) { externalServiceToConnect[ServiceManager.Get<IVoicemodService>()] = null; }
                if (ChannelSession.Settings.EnableCrowdControl) { externalServiceToConnect[ServiceManager.Get<CrowdControlService>()] = null; }
                if (ChannelSession.Settings.EnableSAMMI) { externalServiceToConnect[ServiceManager.Get<SAMMIService>()] = null; }
                if (ServiceManager.Get<IOBSStudioService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<IOBSStudioService>()] = null; }
                if (ServiceManager.Get<StreamlabsDesktopService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<StreamlabsDesktopService>()] = null; }
                if (ServiceManager.Get<XSplitService>().IsEnabled) { externalServiceToConnect[ServiceManager.Get<XSplitService>()] = null; }
                if (!string.IsNullOrEmpty(ChannelSession.Settings.OvrStreamServerIP)) { externalServiceToConnect[ServiceManager.Get<IOvrStreamService>()] = null; }
                if (ChannelSession.Settings.PolyPopPortNumber > 0) { externalServiceToConnect[ServiceManager.Get<PolyPopService>()] = null; }
                if (ChannelSession.Settings.TTSMonsterOAuthToken != null) { externalServiceToConnect[ServiceManager.Get<ITTSMonsterService>()] = ChannelSession.Settings.TTSMonsterOAuthToken; }
                if (ChannelSession.Settings.VTSPogEnabled) { externalServiceToConnect[ServiceManager.Get<VTSPogService>()] = null; }
                if (ChannelSession.Settings.EnableOverlay) { externalServiceToConnect[ServiceManager.Get<OverlayV3Service>()] = null; }
                if (ChannelSession.Settings.MtionStudioEnabled) { externalServiceToConnect[ServiceManager.Get<MtionStudioService>()] = null; }
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
                                Result result = await kvp.Key.Connect();
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
                await ServiceManager.Get<StatisticsService>().Initialize();

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

                ServiceManager.Get<ITelemetryService>().TrackLogin(ChannelSession.Settings.TelemetryUserID, StreamingPlatforms.GetConnectedPlatforms());

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
            ServiceManager.ServiceDisconnect(serviceName);
        }

        public static void ReconnectionOccurred(string serviceName)
        {
            Logger.ForceLog(LogLevel.Information, serviceName + " Service reconnection successful");
            ServiceManager.ServiceReconnect(serviceName);
        }

        internal static void SetChannelSessionSettings(SettingsV3Model settings)
        {
            ChannelSession.Settings = settings;
        }

        private static async Task SessionBackgroundTask(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                sessionBackgroundTimer++;

                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(p);
                    if (session.IsConnected)
                    {
                        await session.RefreshOAuthTokenIfCloseToExpiring();

                        await session.RefreshDetails();
                    }
                });

                foreach (OAuthServiceBase oauthService in ServiceManager.GetAll<OAuthServiceBase>())
                {
                    if (oauthService.IsConnected)
                    {
                        await oauthService.RefreshOAuthTokenIfCloseToExpiring();
                    }
                }

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