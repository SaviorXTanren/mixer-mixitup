using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLabelDisplayV3TypeEnum
    {
        ViewerCount,
        ChatterCount,
        LatestFollower,
        TotalFollowers,
        LatestSubscriber,
        TotalSubscribers,
        LatestRaid,
        LatestDonation,
        LatestTwitchBits,
        LatestTrovoElixir,
        LatestYouTubeSuperChat,
        LatestSubscriptionGifter,

        Counter = 100,

        File = 200,
    }

    public enum OverlayLabelDisplayV3SettingTypeEnum
    {
        RotatingDisplays,
        NewestOnly,
    }

    [DataContract]
    public class OverlayLabelDisplayV3Model
    {
        [DataMember]
        public OverlayLabelDisplayV3TypeEnum Type { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public Guid UserID { get; set; }
        [DataMember]
        public UserV2Model UserFallback { get; set; }

        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string AmountText { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        public void ResetData()
        {
            this.UserID = Guid.Empty;
            this.UserFallback = null;
            this.Amount = 0;
            this.AmountText = string.Empty;
        }
    }

    [DataContract]
    public class OverlayLabelV3Model : OverlayVisualTextV3ModelBase
    {
        public const string UsernamePropertyName = "Username";
        public const string AmountPropertyName = "Amount";
        public const string TypeNamePropertyName = "TypeName";

        public static readonly string DefaultHTML = OverlayResources.OverlayLabelDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayLabelDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayLabelDefaultJavascript;

        [DataMember]
        public OverlayLabelDisplayV3SettingTypeEnum DisplaySetting { get; set; }

        [DataMember]
        public int DisplayRotationSeconds { get; set; }

        [DataMember]
        public Dictionary<OverlayLabelDisplayV3TypeEnum, OverlayLabelDisplayV3Model> Displays { get; set; } = new Dictionary<OverlayLabelDisplayV3TypeEnum, OverlayLabelDisplayV3Model>();

        private CancellationTokenSource refreshCancellationTokenSource;

        private FileSystemWatcher fileSystemWatcher = null;

        public OverlayLabelV3Model() : base(OverlayItemV3Type.Label) { }

        public override async Task Initialize()
        {
            if (string.Equals(this.Javascript, OverlayResources.OverlayLabelDefaultJavascriptOld, System.StringComparison.OrdinalIgnoreCase))
            {
                this.Javascript = OverlayResources.OverlayLabelDefaultJavascript;
            }

            await base.Initialize();

            this.RemoveEventHandlers();

            if (this.Displays.Count == 0)
            {
                return;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.ViewerCount) || this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.ChatterCount))
            {
                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    do
                    {
                        if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.ViewerCount))
                        {
                            this.Displays[OverlayLabelDisplayV3TypeEnum.ViewerCount].Amount = ServiceManager.Get<ChatService>().GetViewerCount();
                            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.ViewerCount);
                        }

                        if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.ChatterCount))
                        {
                            this.Displays[OverlayLabelDisplayV3TypeEnum.ChatterCount].Amount = ServiceManager.Get<UserService>().ActiveUserCount;
                            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.ChatterCount);
                        }

                        await Task.Delay(60000);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestFollower) || this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalFollowers))
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;

                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestFollower) && this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].UserID == Guid.Empty)
                {
                    UserV2ViewModel user = null;
                    if (ChannelSession.Settings.LastFollowerUserID != Guid.Empty)
                    {
                        user = await ServiceManager.Get<UserService>().GetUserByID(ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.Settings.LastFollowerUserID);
                    }

                    if (user == null)
                    {
                        if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                        {
                            var followers = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIFollowers(ServiceManager.Get<TwitchSession>().StreamerModel, maxResults: 1);
                            if (followers != null && followers.Count() > 0)
                            {
                                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: followers.First().user_id, performPlatformSearch: true);
                            }
                        }
                        else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSession>().IsConnected)
                        {
                            var followers = await ServiceManager.Get<TrovoSession>().StreamerService.GetFollowers(ServiceManager.Get<TrovoSession>().ChannelID, maxResults: 1);
                            if (followers != null && followers.Count() > 0)
                            {
                                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: followers.First().user_id, platformUsername: followers.First().nickname, performPlatformSearch: true);
                            }
                        }
                    }

                    if (user != null)
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].UserID = user.ID;
                    }
                }

                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalFollowers))
                {
                    long amount = 0;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                    {
                        amount = await ServiceManager.Get<TwitchSession>().StreamerService.GetFollowerCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSession>().IsConnected)
                    {
                        amount = (await ServiceManager.Get<TrovoSession>().StreamerService.GetFollowers(ServiceManager.Get<TrovoSession>().ChannelID, int.MaxValue)).Count();
                    }
                    this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].Amount = amount;
                }
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestRaid))
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber) || this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers) || this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter))
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;
                EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;

                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber) && this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID == Guid.Empty)
                {
                    UserV2ViewModel user = null;
                    if (ChannelSession.Settings.LastSubscriberUserID != Guid.Empty)
                    {
                        user = await ServiceManager.Get<UserService>().GetUserByID(ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.Settings.LastSubscriberUserID);
                    }

                    if (user == null)
                    {
                        if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                        {
                            var subscribers = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscribers(ServiceManager.Get<TwitchSession>().StreamerModel, maxResults: 1);
                            if (subscribers != null && subscribers.Count() > 0)
                            {
                                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscribers.First().user_id, performPlatformSearch: true);
                            }
                        }
                    }

                    if (user != null)
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = user.ID;
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].AmountText = " ";
                    }
                }

                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers))
                {
                    long amount = 0;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                    {
                        amount = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSession>().IsConnected)
                    {
                        amount = (await ServiceManager.Get<TrovoSession>().StreamerService.GetSubscribers(ServiceManager.Get<TrovoSession>().ChannelID, int.MaxValue)).Count();
                    }
                    this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount = amount;
                }
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestDonation))
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestTwitchBits))
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir))
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat))
            {
                EventService.OnYouTubeSuperChatOccurred += EventService_OnYouTubeSuperChatOccurred;
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.Counter))
            {
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
                if (!string.IsNullOrEmpty(this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName) &&
                    ChannelSession.Settings.Counters.TryGetValue(this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName, out CounterModel counter))
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].Amount = counter.Amount;
                }
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.File))
            {
                if (this.fileSystemWatcher != null)
                {
                    this.fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                    this.fileSystemWatcher.Dispose();
                    this.fileSystemWatcher = null;
                }

                string filePath = this.Displays[OverlayLabelDisplayV3TypeEnum.File].FilePath;
                if (ServiceManager.Get<IFileService>().FileExists(filePath))
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.File].Format = await ServiceManager.Get<IFileService>().ReadFile(filePath);

                    this.fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath));
                    this.fileSystemWatcher.Filter = Path.GetFileName(filePath);
                    this.fileSystemWatcher.Changed += FileSystemWatcher_Changed;
                    this.fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            this.RemoveEventHandlers();

            if (this.fileSystemWatcher != null)
            {
                this.fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                this.fileSystemWatcher.Dispose();
                this.fileSystemWatcher = null;
            }
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.DisplaySetting)] = this.DisplaySetting.ToString();
            properties[nameof(this.DisplayRotationSeconds)] = this.DisplayRotationSeconds;
            return properties;
        }

        protected override async Task Loaded()
        {
            await base.Loaded();

            foreach (var display in this.Displays)
            {
                if (display.Value.IsEnabled)
                {
                    try
                    {
                        Dictionary<string, object> data = await this.GetLabelDisplayProperties(display.Value);
                        await this.CallFunction("add", data);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
            }
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestFollower))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].UserID = user.ID;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestFollower);
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalFollowers))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalFollowers);
            }
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestRaid].UserID = raid.Item1.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestRaid].Amount = raid.Item2;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestRaid);
        }

        private async void EventService_OnSubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = 1;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }
        }

        private async void EventService_OnResubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = subscription.Months;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = 1;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }

            if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter) && subscription.Gifter != null)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].ResetData();
                if (subscription.Gifter.IsUnassociated)
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].UserFallback = subscription.Gifter.Model;
                }
                else
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].UserID = subscription.Gifter.ID;
                }

                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].Amount = 1;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter);
            }
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            if (subscriptions != null && subscriptions.Count() > 0)
            {
                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriber))
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscriptions.Last().User.ID;
                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = 1;
                    await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
                }

                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.TotalSubscribers))
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount += subscriptions.Count();
                    await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
                }

                UserV2ViewModel gifter = subscriptions.Last().Gifter;
                if (this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter) && gifter != null)
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].ResetData();
                    if (gifter.IsUnassociated)
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].UserFallback = gifter.Model;
                    }
                    else
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].UserID = gifter.ID;
                    }

                    this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter].Amount = subscriptions.Count();
                    await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriptionGifter);
                }
            }
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].ResetData();

            if (donation.IsAnonymous || donation.User.IsUnassociated)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].UserFallback = donation.User.Model;
            }
            else
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].UserID = donation.User.ID;
            }

            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].Amount = donation.Amount;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestDonation);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchBitsCheeredEventModel bitsCheered)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTwitchBits].UserID = bitsCheered.User.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTwitchBits].Amount = bitsCheered.Amount;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestTwitchBits);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir].UserID = spell.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir].Amount = spell.ValueTotal;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir);
            }
        }

        private async void EventService_OnYouTubeSuperChatOccurred(object sender, YouTubeSuperChatViewModel superChat)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat].UserID = superChat.User.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat].AmountText = superChat.AmountDisplay;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat);
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (string.Equals(counter.Name, this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName, StringComparison.OrdinalIgnoreCase))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].Amount = counter.Amount;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.Counter);
            }
        }

        private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed && this.IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum.File))
            {
                // Attempt to read from the file up to 5 times with a 1 second delay in-between each attempt in the event there's a file lock
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.File].Format = await ServiceManager.Get<IFileService>().ReadFile(e.FullPath);
                        await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.File);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    await Task.Delay(1000);
                }
            }
        }

        private async Task SendUpdate(OverlayLabelDisplayV3TypeEnum type)
        {
            OverlayLabelDisplayV3Model display = this.Displays[type];
            if (display.IsEnabled)
            {
                try
                {
                    Dictionary<string, object> data = await this.GetLabelDisplayProperties(display);
                    await this.CallFunction("update", data);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        private async Task<Dictionary<string, object>> GetLabelDisplayProperties(OverlayLabelDisplayV3Model display)
        {
            UserV2ViewModel user = null;
            if (display.UserFallback != null)
            {
                user = new UserV2ViewModel(display.UserFallback);
            }
            else if (display.UserID != Guid.Empty)
            {
                user = await ServiceManager.Get<UserService>().GetUserByID(StreamingPlatformTypeEnum.All, display.UserID);
            }

            if (user == null)
            {
                user = ChannelSession.User;
            }

            string amount = display.Amount.ToString();
            if (!string.IsNullOrEmpty(display.AmountText))
            {
                amount = display.AmountText;
            }

            string result = display.Format;
            if (user != null)
            {
                result = OverlayV3Service.ReplaceProperty(result, OverlayLabelV3Model.UsernamePropertyName, user.DisplayName);
            }
            if (!string.IsNullOrEmpty(amount))
            {
                result = OverlayV3Service.ReplaceProperty(result, OverlayLabelV3Model.AmountPropertyName, amount);
            }
            result = OverlayV3Service.ReplaceProperty(result, OverlayLabelV3Model.TypeNamePropertyName, EnumLocalizationHelper.GetLocalizedName(display.Type));

            result = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result, new CommandParametersModel(user));

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[nameof(display.Type)] = display.Type.ToString();
            data[OverlayLabelV3Model.TypeNamePropertyName] = EnumLocalizationHelper.GetLocalizedName(display.Type);
            data[nameof(display.Format)] = result;
            data["User"] = user;

            return data;
        }

        private bool IsDisplayEnabled(OverlayLabelDisplayV3TypeEnum type)
        {
            return this.Displays.TryGetValue(type, out OverlayLabelDisplayV3Model display) && display.IsEnabled;
        }

        private void RemoveEventHandlers()
        {
            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            EventService.OnYouTubeSuperChatOccurred -= EventService_OnYouTubeSuperChatOccurred;
            CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
        }
    }
}
