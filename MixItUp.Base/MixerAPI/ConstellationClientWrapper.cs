using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        public ConstellationClient Client { get; private set; }

        public IReadOnlyDictionary<Guid, SkillModel> AvailableSkills { get { return this.availableSkills; } }

        private LockedDictionary<string, LockedHashSet<uint>> userEventTracking = new LockedDictionary<string, LockedHashSet<uint>>();

        private List<PatronageMilestoneModel> allPatronageMilestones = new List<PatronageMilestoneModel>();
        private List<PatronageMilestoneModel> remainingPatronageMilestones = new List<PatronageMilestoneModel>();
        private SemaphoreSlim patronageMilestonesSemaphore = new SemaphoreSlim(1);

        private SkillCatalogModel skillCatalog;
        private Dictionary<Guid, SkillModel> availableSkills = new Dictionary<Guid, SkillModel>();

        public ConstellationClientWrapper()
        {
            GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnSkillUseOccurred += GlobalEvents_OnSkillUseOccurred;
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
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

            // Hacky workaround until auth issue is fixed for Skill Catalog
            // this.skillCatalog = await ChannelSession.Connection.GetSkillCatalog(ChannelSession.MixerChannel);
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string data = await httpClient.GetStringAsync("https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/MixItUp.Base/SkillsCatalogData.txt");
                    if (!string.IsNullOrEmpty(data))
                    {
                        this.skillCatalog = SerializerHelper.DeserializeFromString<SkillCatalogModel>(data);
                        if (this.skillCatalog != null)
                        {
                            this.availableSkills = new Dictionary<Guid, SkillModel>(this.skillCatalog.skills.ToDictionary(s => s.id, s => s));
                        }
                    }
                }
            }
            catch (Exception ex) { Util.Logger.Log(ex); }

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

        public bool CanUserRunEvent(UserViewModel user, string eventName)
        {
            return (!this.userEventTracking.ContainsKey(eventName) || !this.userEventTracking[eventName].Contains(user.ID));
        }

        public void LogUserRunEvent(UserViewModel user, string eventName)
        {
            if (!this.userEventTracking.ContainsKey(eventName))
            {
                this.userEventTracking[eventName] = new LockedHashSet<uint>();
            }

            if (user != null)
            {
                this.userEventTracking[eventName].Add(user.ID);
            }
        }

        public EventCommand FindMatchingEventCommand(string eventDetails)
        {
            foreach (EventCommand command in ChannelSession.Settings.EventCommands)
            {
                if (command.MatchesEvent(eventDetails))
                {
                    return command;
                }
            }
            return null;
        }

        public async Task RunEventCommand(EventCommand command, UserViewModel user, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            if (command != null)
            {
                if (user != null)
                {
                    await command.Perform(user, arguments: arguments, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
                else
                {
                    await command.Perform(await ChannelSession.GetCurrentUser(), arguments: arguments, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
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
                    user = new UserViewModel(payloadToken.ToObject<UserModel>());

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
                    user = new UserViewModel(channel.userId, channel.token);
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
                            if (this.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStart)))
                            {
                                this.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStart));
                                await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStart)), user);
                            }
                        }
                        else
                        {
                            if (this.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStop)))
                            {
                                this.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStop));
                                await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChannelStreamStop)), user);
                            }
                        }
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()))
                {
                    if (followed.GetValueOrDefault())
                    {
                        if (this.CanUserRunEvent(user, ConstellationClientWrapper.ChannelFollowEvent.ToString()))
                        {
                            this.LogUserRunEvent(user, ConstellationClientWrapper.ChannelFollowEvent.ToString());

                            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                            {
                                user.Data.AddCurrencyAmount(currency, currency.OnFollowBonus);
                            }

                            await this.RunEventCommand(this.FindMatchingEventCommand(e.channel), user);
                        }

                        GlobalEvents.FollowOccurred(user);
                    }
                    else
                    {
                        if (this.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserUnfollow)))
                        {
                            this.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserUnfollow));
                            await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserUnfollow)), user);
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

                    if (this.CanUserRunEvent(user, ConstellationClientWrapper.ChannelHostedEvent.ToString()))
                    {
                        this.LogUserRunEvent(user, ConstellationClientWrapper.ChannelHostedEvent.ToString());

                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnHostBonus);
                        }

                        EventCommand command = this.FindMatchingEventCommand(e.channel);
                        if (command != null)
                        {
                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>() { { "hostviewercount", viewerCount.ToString() } };
                            await this.RunEventCommand(command, user, extraSpecialIdentifiers: specialIdentifiers);
                        }

                        GlobalEvents.HostOccurred(new Tuple<UserViewModel, int>(user, viewerCount));
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
                {
                    if (this.CanUserRunEvent(user, ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
                    {
                        this.LogUserRunEvent(user, ConstellationClientWrapper.ChannelSubscribedEvent.ToString());

                        user.MixerSubscribeDate = DateTimeOffset.Now;
                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                        }

                        await this.RunEventCommand(this.FindMatchingEventCommand(e.channel), user);
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

                    if (this.CanUserRunEvent(user, ConstellationClientWrapper.ChannelResubscribedEvent.ToString()))
                    {
                        this.LogUserRunEvent(user, ConstellationClientWrapper.ChannelResubscribedEvent.ToString());

                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                        }

                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>() { { "usersubmonths", resubMonths.ToString() } };
                        await this.RunEventCommand(this.FindMatchingEventCommand(ConstellationClientWrapper.ChannelResubscribedEvent.ToString()), user, extraSpecialIdentifiers: specialIdentifiers);

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

                            EventCommand command = this.FindMatchingEventCommand(e.channel);
                            if (command != null)
                            {
                                await this.RunEventCommand(command, gifterUser, arguments: new List<string>() { receiverUser.UserName });
                            }

                            GlobalEvents.SubscriptionGiftedOccurred(gifterUser, receiverUser);
                        }
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ProgressionLevelupEvent.ToString()))
                {
                    if (e.payload.TryGetValue("userId", out JToken userID))
                    {
                        UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(userID.ToObject<uint>());
                        if (userModel != null)
                        {
                            UserViewModel userViewModel = new UserViewModel(userModel);
                            UserFanProgressionModel fanProgression = e.payload.ToObject<UserFanProgressionModel>();
                            if (fanProgression != null)
                            {
                                userViewModel.FanProgression = fanProgression;
                                EventCommand command = this.FindMatchingEventCommand(e.channel);
                                if (command != null)
                                {
                                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                                    {
                                        { "userfanprogressionnext", fanProgression.level.nextLevelXp.ToString() },
                                        { "userfanprogressionrank", fanProgression.level.level.ToString() },
                                        { "userfanprogressioncolor", fanProgression.level.color.ToString() },
                                        { "userfanprogressionimage", fanProgression.level.LargeGIFAssetURL.ToString() },
                                        { "userfanprogression", fanProgression.level.currentXp.ToString() },
                                    };
                                    await this.RunEventCommand(command, userViewModel, extraSpecialIdentifiers: specialIdentifiers);
                                }

                                foreach (UserCurrencyViewModel fanProgressionCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingFanProgression))
                                {
                                    userViewModel.Data.SetCurrencyAmount(fanProgressionCurrency, (int)fanProgression.level.level);
                                }

                                GlobalEvents.ProgressionLevelUpOccurred(userViewModel);
                            }
                        }
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSkillEvent.ToString()))
                {
                    if (e.payload["triggeringUserId"] != null)
                    {
                        uint userID = e.payload["triggeringUserId"].ToObject<uint>();
                        user = await ChannelSession.ActiveUsers.GetUserByID(userID);
                        if (user != null)
                        {
                            user = new UserViewModel(await ChannelSession.MixerStreamerConnection.GetUser(userID));
                        }
                    }

                    SkillModel skill = null;
                    if (e.payload["skillId"] != null)
                    {
                        Guid skillID = e.payload["skillId"].ToObject<Guid>();
                        if (this.availableSkills.ContainsKey(skillID))
                        {
                            skill = this.availableSkills[skillID];
                        }
                    }

                    if (skill == null)
                    {
                        if (e.payload["manifest"] != null && e.payload["manifest"]["name"] != null)
                        {
                            string skillName = e.payload["manifest"]["name"].ToString();
                            if (skillName.Equals("beachball"))
                            {
                                skill = this.availableSkills.Values.FirstOrDefault(s => s.name.Equals("Beach Ball"));
                            }
                        }
                    }

                    uint price = e.payload["price"].ToObject<uint>();
                    if (user != null)
                    {
                        if (price > 0)
                        {
                            GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, int>(user, (int)price));
                        }

                        if (skill != null)
                        {
                            JObject manifest = (JObject)e.payload["manifest"];
                            JObject parameters = (JObject)e.payload["parameters"];
                            SkillInstanceModel skillInstance = new SkillInstanceModel(skill, manifest, parameters);

                            GlobalEvents.SkillUseOccurred(new SkillUsageModel(user, skillInstance));
                        }
                    }
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
                                    { SpecialIdentifierStringBuilder.MilestoneSpecialIdentifierHeader + "reward", milestoneReached.DollarAmountText() },
                                };
                                await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerMilestoneReached)), await ChannelSession.GetCurrentUser(), extraSpecialIdentifiers: specialIdentifiers);
                            }
                        }
                    }
                }

                if (this.OnEventOccurred != null)
                {
                    this.OnEventOccurred(this, e);
                }
            }
            catch (Exception ex) { Util.Logger.Log(ex); }
        }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> sparkUsage)
        {
            foreach (UserCurrencyViewModel sparkCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingSparks))
            {
                sparkUsage.Item1.Data.AddCurrencyAmount(sparkCurrency, sparkUsage.Item2);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
            {
                { "sparkamount", sparkUsage.Item2.ToString() },
            };

            await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerSparksUsed)), sparkUsage.Item1, extraSpecialIdentifiers: specialIdentifiers);
        }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage)
        {
            foreach (UserCurrencyViewModel emberCurrency in ChannelSession.Settings.Currencies.Values.Where(c => c.IsTrackingEmbers))
            {
                emberUsage.User.Data.AddCurrencyAmount(emberCurrency, emberUsage.Amount);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
            {
                { "emberamount", emberUsage.Amount.ToString() },
            };

            await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerEmbersUsed)), emberUsage.User, extraSpecialIdentifiers: specialIdentifiers);
        }

        private async void GlobalEvents_OnSkillUseOccurred(object sender, SkillUsageModel skill)
        {
            await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerSkillUsed)), skill.User, extraSpecialIdentifiers: skill.GetSpecialIdentifiers());
        }

        private async void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsWhisper && !message.IsAlert)
            {
                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                {
                    { "message", message.PlainTextMessage },
                };

                await this.RunEventCommand(this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChatMessage)), message.User, extraSpecialIdentifiers: specialIdentifiers);
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
