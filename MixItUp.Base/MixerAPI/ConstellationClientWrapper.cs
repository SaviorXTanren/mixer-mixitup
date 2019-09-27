using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ConstellationClientWrapper : MixerWebSocketWrapper
    {
        public static ConstellationEventType ChannelUpdateEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__update, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelSubscriptionGiftedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscriptionGifted, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelSkillEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__skill, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ChannelPatronageUpdateEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__patronageUpdate, ChannelSession.MixerChannel.id); } }
        public static ConstellationEventType ProgressionLevelupEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.progression__id__levelup, ChannelSession.MixerChannel.id); } }

        private static readonly List<ConstellationEventTypeEnum> subscribedEvents = new List<ConstellationEventTypeEnum>()
        {
            ConstellationEventTypeEnum.channel__id__followed, ConstellationEventTypeEnum.channel__id__hosted, ConstellationEventTypeEnum.channel__id__subscribed,
            ConstellationEventTypeEnum.channel__id__resubscribed, ConstellationEventTypeEnum.channel__id__resubShared, ConstellationEventTypeEnum.channel__id__subscriptionGifted,
            ConstellationEventTypeEnum.channel__id__update, ConstellationEventTypeEnum.channel__id__skill, ConstellationEventTypeEnum.channel__id__patronageUpdate,
            ConstellationEventTypeEnum.progression__id__levelup,
        };

        public event EventHandler<ConstellationLiveEventModel> OnEventOccurred;

        public LockedDictionary<Guid, MixerSkillPayloadModel> SkillEventsTriggered { get; private set; } = new LockedDictionary<Guid, MixerSkillPayloadModel>();

        public ConstellationClient Client { get; private set; }

        private List<PatronageMilestoneModel> allPatronageMilestones = new List<PatronageMilestoneModel>();
        private List<PatronageMilestoneModel> remainingPatronageMilestones = new List<PatronageMilestoneModel>();
        private SemaphoreSlim patronageMilestonesSemaphore = new SemaphoreSlim(1);

        public ConstellationClientWrapper()
        {
            GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnSkillUseOccurred += GlobalEvents_OnSkillUseOccurred;
        }

        public async Task<bool> Connect()
        {
            PatronageStatusModel patronageStatus = await ChannelSession.MixerStreamerConnection.GetPatronageStatus(ChannelSession.MixerChannel);
            if (patronageStatus != null)
            {
                PatronagePeriodModel patronagePeriod = await ChannelSession.MixerStreamerConnection.GetPatronagePeriod(patronageStatus);
                if (patronagePeriod != null)
                {
                    this.allPatronageMilestones = new List<PatronageMilestoneModel>(patronagePeriod.milestoneGroups.SelectMany(mg => mg.milestones));
                    this.remainingPatronageMilestones = new List<PatronageMilestoneModel>(this.allPatronageMilestones.Where(m => m.target > patronageStatus.patronageEarned));
                }
            }

            return await this.AttemptConnect();
        }

        public async Task SubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.SubscribeToEvents(events)); }

        public async Task UnsubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.UnsubscribeToEvents(events)); }

        public async Task Disconnect()
        {
            await this.RunAsync(async () =>
            {
                if (this.Client != null)
                {
                    this.Client.OnDisconnectOccurred -= ConstellationClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.Client.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                        this.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                        this.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                        this.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                    }
                    this.Client.OnSubscribedEventOccurred -= ConstellationClient_OnSubscribedEventOccurred;

                    await this.RunAsync(this.Client.Disconnect());

                    this.backgroundThreadCancellationTokenSource.Cancel();
                }
                this.Client = null;
            });
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.Client = await this.RunAsync(ConstellationClient.Create(ChannelSession.MixerStreamerConnection.Connection));
            return await this.RunAsync(async () =>
            {
                if (this.Client != null)
                {
                    this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                    if (await this.RunAsync(this.Client.Connect()))
                    {
                        this.Client.OnDisconnectOccurred += ConstellationClient_OnDisconnectOccurred;
                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.Client.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                            this.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                            this.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                            this.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                        }
                        this.Client.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;

                        await this.SubscribeToEvents(ConstellationClientWrapper.subscribedEvents.Select(e => new ConstellationEventType(e, ChannelSession.MixerChannel.id)));

                        return true;
                    }
                }
                return false;
            });
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            try
            {
                UserViewModel user = null;
                bool? followed = null;
                ChannelModel channel = null;

                JToken payloadToken;
                if (e.payload.TryGetValue("user", out payloadToken))
                {
                    UserModel userPayload = payloadToken.ToObject<UserModel>();
                    user = ChannelSession.Services.User.GetUserByID(userPayload.id.ToString());
                    if (user == null)
                    {
                        user = new UserViewModel(userPayload);
                    }

                    JToken subscribeStartToken;
                    if (e.payload.TryGetValue("since", out subscribeStartToken))
                    {
                        user.MixerSubscribeDate = subscribeStartToken.ToObject<DateTimeOffset>();
                    }

                    if (e.payload.TryGetValue("following", out JToken followedToken))
                    {
                        followed = (bool)followedToken;
                    }
                }
                else if (e.payload.TryGetValue("hoster", out payloadToken))
                {
                    channel = payloadToken.ToObject<ChannelModel>();
                    user = ChannelSession.Services.User.GetUserByID(channel.userId.ToString());
                    if (user == null)
                    {
                        user = new UserViewModel(channel.userId, channel.token);
                    }
                }
                else if (e.payload.TryGetValue("userId", out JToken userID))
                {
                    user = ChannelSession.Services.User.GetUserByID(userID.ToObject<uint>().ToString());
                    if (user == null)
                    {
                        UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(userID.ToObject<uint>());
                        if (userModel != null)
                        {
                            user = new UserViewModel(userModel);
                        }
                    }
                }

                if (user != null)
                {
                    user.UpdateLastActivity();
                }

                if (e.channel.Equals(ConstellationClientWrapper.ChannelUpdateEvent.ToString()))
                {
                    if (e.payload["online"] != null)
                    {
                        bool online = e.payload["online"].ToObject<bool>();
                        user = await ChannelSession.GetCurrentUser();
                        if (online)
                        {
                            if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStart)))
                            {
                                await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStart), user);
                            }
                        }
                        else
                        {
                            if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStop)))
                            {
                                await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStop), user);
                            }
                        }
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()))
                {
                    if (followed.GetValueOrDefault())
                    {
                        if (EventCommand.CanUserRunEvent(user, ConstellationClientWrapper.ChannelFollowEvent.ToString()))
                        {
                            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                            {
                                user.Data.AddCurrencyAmount(currency, currency.OnFollowBonus);
                            }

                            await EventCommand.FindAndRunEventCommand(e.channel, user);

                            await this.AddAlertChatMessage(user, string.Format("{0} Followed", user.UserName));
                        }
                        GlobalEvents.FollowOccurred(user);
                    }
                    else
                    {
                        if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserUnfollow)))
                        {
                            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserUnfollow), user);

                            await this.AddAlertChatMessage(user, string.Format("{0} Unfollowed", user.UserName));
                        }
                        GlobalEvents.UnfollowOccurred(user);
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelHostedEvent.ToString()))
                {
                    int viewerCount = 0;
                    if (channel != null)
                    {
                        viewerCount = (int)channel.viewersCurrent;
                    }

                    if (EventCommand.CanUserRunEvent(user, ConstellationClientWrapper.ChannelHostedEvent.ToString()))
                    {
                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnHostBonus);
                        }

                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>() { { "hostviewercount", viewerCount.ToString() } };
                        await EventCommand.FindAndRunEventCommand(e.channel, user, extraSpecialIdentifiers: specialIdentifiers);

                        await this.AddAlertChatMessage(user, string.Format("{0} Hosted With {1} Viewers", user.UserName, viewerCount));

                        GlobalEvents.HostOccurred(new Tuple<UserViewModel, int>(user, viewerCount));
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
                {
                    if (EventCommand.CanUserRunEvent(user, ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
                    {
                        user.MixerSubscribeDate = DateTimeOffset.Now;
                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                        }

                        await EventCommand.FindAndRunEventCommand(e.channel, user);

                        await this.AddAlertChatMessage(user, string.Format("{0} Subscribed", user.UserName));
                    }

                    GlobalEvents.SubscribeOccurred(user);
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedEvent.ToString()) || e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedSharedEvent.ToString()))
                {
                    int resubMonths = 0;
                    if (e.payload.TryGetValue("totalMonths", out JToken resubMonthsToken))
                    {
                        resubMonths = (int)resubMonthsToken;
                    }

                    if (EventCommand.CanUserRunEvent(user, ConstellationClientWrapper.ChannelResubscribedEvent.ToString()))
                    {
                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                        }

                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>() { { "usersubmonths", resubMonths.ToString() } };
                        await EventCommand.FindAndRunEventCommand(ConstellationClientWrapper.ChannelResubscribedEvent.ToString(), user, extraSpecialIdentifiers: specialIdentifiers);

                        await this.AddAlertChatMessage(user, string.Format("{0} Re-Subscribed For {1} Months", user.UserName, resubMonths));

                        GlobalEvents.ResubscribeOccurred(new Tuple<UserViewModel, int>(user, resubMonths));
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscriptionGiftedEvent.ToString()))
                {
                    if (e.payload.TryGetValue("gifterId", out JToken gifterID) && e.payload.TryGetValue("giftReceiverId", out JToken receiverID))
                    {
                        UserModel gifterUserModel = await ChannelSession.MixerStreamerConnection.GetUser(gifterID.ToObject<uint>());
                        UserModel receiverUserModel = await ChannelSession.MixerStreamerConnection.GetUser(receiverID.ToObject<uint>());
                        if (gifterUserModel != null && receiverUserModel != null)
                        {
                            UserViewModel gifterUser = new UserViewModel(gifterUserModel);
                            UserViewModel receiverUser = new UserViewModel(receiverUserModel);

                            await EventCommand.FindAndRunEventCommand(e.channel, gifterUser, arguments: new List<string>() { receiverUser.UserName });

                            await this.AddAlertChatMessage(gifterUser, string.Format("{0} Gifted A Subscription To {1}", gifterUser.UserName, receiverUser.UserName));

                            GlobalEvents.SubscriptionGiftedOccurred(gifterUser, receiverUser);
                        }
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ProgressionLevelupEvent.ToString()))
                {
                    UserFanProgressionModel fanProgression = e.payload.ToObject<UserFanProgressionModel>();
                    if (fanProgression != null)
                    {
                        user.FanProgression = fanProgression;
                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                                {
                                    { "userfanprogressionnext", fanProgression.level.nextLevelXp.ToString() },
                                    { "userfanprogressionrank", fanProgression.level.level.ToString() },
                                    { "userfanprogressioncolor", fanProgression.level.color.ToString() },
                                    { "userfanprogressionimage", fanProgression.level.LargeGIFAssetURL.ToString() },
                                    { "userfanprogression", fanProgression.level.currentXp.ToString() },
                                };

                        await EventCommand.FindAndRunEventCommand(e.channel, user, extraSpecialIdentifiers: specialIdentifiers);

                        foreach (UserCurrencyViewModel fanProgressionCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingFanProgression))
                        {
                            user.Data.SetCurrencyAmount(fanProgressionCurrency, (int)fanProgression.level.level);
                        }

                        GlobalEvents.ProgressionLevelUpOccurred(user);
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSkillEvent.ToString()))
                {
                    MixerSkillPayloadModel skillPayload = e.payload.ToObject<MixerSkillPayloadModel>();
                    this.SkillEventsTriggered[skillPayload.executionId] = skillPayload;
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelPatronageUpdateEvent.ToString()))
                {
                    PatronageStatusModel patronageStatus = e.payload.ToObject<PatronageStatusModel>();
                    if (patronageStatus != null)
                    {
                        GlobalEvents.PatronageUpdateOccurred(patronageStatus);

                        bool milestoneUpdateOccurred = await this.patronageMilestonesSemaphore.WaitAndRelease(() =>
                        {
                            return Task.FromResult(this.remainingPatronageMilestones.RemoveAll(m => m.target <= patronageStatus.patronageEarned) > 0);
                        });

                        if (milestoneUpdateOccurred)
                        {
                            PatronageMilestoneModel milestoneReached = this.allPatronageMilestones.OrderByDescending(m => m.target).FirstOrDefault(m => m.target <= patronageStatus.patronageEarned);
                            if (milestoneReached != null)
                            {
                                GlobalEvents.PatronageMilestoneReachedOccurred(milestoneReached);

                                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                                {
                                    { SpecialIdentifierStringBuilder.MilestoneSpecialIdentifierHeader + "amount", milestoneReached.target.ToString() },
                                    { SpecialIdentifierStringBuilder.MilestoneSpecialIdentifierHeader + "reward", milestoneReached.PercentageAmountText() },
                                };
                                await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerMilestoneReached), await ChannelSession.GetCurrentUser(), extraSpecialIdentifiers: specialIdentifiers);
                            }
                        }
                    }
                }

                if (this.OnEventOccurred != null)
                {
                    this.OnEventOccurred(this, e);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage)
        {
            foreach (UserCurrencyViewModel sparkCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingSparks))
            {
                sparkUsage.Item1.Data.AddCurrencyAmount(sparkCurrency, (int)sparkUsage.Item2);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
            {
                { "sparkamount", sparkUsage.Item2.ToString() },
            };

            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerSparksUsed), sparkUsage.Item1, extraSpecialIdentifiers: specialIdentifiers);
        }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage)
        {
            foreach (UserCurrencyViewModel emberCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingEmbers))
            {
                emberUsage.User.Data.AddCurrencyAmount(emberCurrency, (int)emberUsage.Amount);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
            {
                { "emberamount", emberUsage.Amount.ToString() },
            };

            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerEmbersUsed), emberUsage.User, extraSpecialIdentifiers: specialIdentifiers);
        }

        private async void GlobalEvents_OnSkillUseOccurred(object sender, MixerSkillChatMessageViewModel skill)
        {
            Dictionary<string, string> specialIdentifiers = skill.Skill.GetSpecialIdentifiers();
            specialIdentifiers["skillmessage"] = skill.PlainTextMessage;
            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerSkillUsed), skill.User, extraSpecialIdentifiers: specialIdentifiers);
        }

        private async Task AddAlertChatMessage(UserViewModel user, string message)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Mixer, user, message, ChannelSession.Settings.ChatEventAlertsColorScheme));
            }
        }

        private async void ConstellationClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Constellation");

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.Connect());

            ChannelSession.ReconnectionOccurred("Constellation");
        }
    }
}
