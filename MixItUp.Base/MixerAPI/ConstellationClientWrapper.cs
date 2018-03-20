using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
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
        private static SemaphoreSlim reconnectionLock = new SemaphoreSlim(1);

        public static ConstellationEventType ChannelUpdateEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__update, ChannelSession.Channel.id); } }

        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        private static readonly List<ConstellationEventTypeEnum> subscribedEvents = new List<ConstellationEventTypeEnum>()
        {
            ConstellationEventTypeEnum.channel__id__followed, ConstellationEventTypeEnum.channel__id__hosted, ConstellationEventTypeEnum.channel__id__subscribed,
            ConstellationEventTypeEnum.channel__id__resubscribed, ConstellationEventTypeEnum.channel__id__resubShared, ConstellationEventTypeEnum.channel__id__update
        };

        public event EventHandler<ConstellationLiveEventModel> OnEventOccurred;

        public event EventHandler<UserViewModel> OnFollowOccurred;
        public event EventHandler<UserViewModel> OnUnfollowOccurred;
        public event EventHandler<Tuple<UserViewModel, int>> OnHostedOccurred;
        public event EventHandler<UserViewModel> OnSubscribedOccurred;
        public event EventHandler<Tuple<UserViewModel, int>> OnResubscribedOccurred;

        public ConstellationClient Client { get; private set; }

        private HashSet<uint> userFollows = new HashSet<uint>();

        public ConstellationClientWrapper()
        {
            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;

            this.OnPingDisconnectOcurred += Client_OnPingDisconnectOcurred;
        }

        public async Task<bool> Connect()
        {
            return await this.AttemptConnect();
        }

        public async Task SubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.SubscribeToEvents(events)); }

        public async Task UnsubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.UnsubscribeToEvents(events)); }

        public async Task Disconnect()
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
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.Client = await this.RunAsync(ConstellationClient.Create(ChannelSession.Connection.Connection));
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

                    await this.SubscribeToEvents(ConstellationClientWrapper.subscribedEvents.Select(e => new ConstellationEventType(e, ChannelSession.Channel.id)));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await this.PingChecker(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
            }
            return false;
        }

        protected override async Task<bool> Ping()
        {
            if (this.Client != null)
            {
                return await this.RunAsync(this.Client.Ping());
            }
            return true;
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            UserViewModel user = null;
            bool? followed = null;
            ChannelModel channel = null;

            JToken userToken;
            if (e.payload.TryGetValue("user", out userToken))
            {
                user = new UserViewModel(userToken.ToObject<UserModel>());

                JToken subscribeStartToken;
                if (e.payload.TryGetValue("since", out subscribeStartToken))
                {
                    user.SubscribeDate = subscribeStartToken.ToObject<DateTimeOffset>();
                }

                if (e.payload.TryGetValue("following", out JToken followedToken))
                {
                    followed = (bool)followedToken;
                }
            }
            else if (e.payload.TryGetValue("hoster", out userToken))
            {
                channel = userToken.ToObject<ChannelModel>();
                user = new UserViewModel(channel.id, channel.token);
            }

            if (e.channel.Equals(ConstellationClientWrapper.ChannelUpdateEvent.ToString()))
            {

            }
            else if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()))
            {
                if (followed.GetValueOrDefault())
                {
                    if (!this.userFollows.Contains(user.ID))
                    {
                        this.userFollows.Add(user.ID);

                        foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.OnFollowBonus);
                        }

                        if (this.OnFollowOccurred != null)
                        {
                            this.OnFollowOccurred(this, user);
                        }

                        await this.RunEventCommand(this.FindMatchingEventCommand(e.channel), user);
                    }
                }
                else
                {
                    if (this.OnUnfollowOccurred != null)
                    {
                        this.OnUnfollowOccurred(this, user);
                    }
                }
            }
            else if (e.channel.Equals(ConstellationClientWrapper.ChannelHostedEvent.ToString()))
            {
                int viewerCount = 0;
                if (channel != null)
                {
                    viewerCount = (int)channel.viewersCurrent;
                }

                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    user.Data.AddCurrencyAmount(currency, currency.OnHostBonus);
                }

                if (this.OnHostedOccurred != null)
                {
                    this.OnHostedOccurred(this, new Tuple<UserViewModel, int>(user, viewerCount));
                }

                EventCommand command = this.FindMatchingEventCommand(e.channel);
                if (command != null)
                {
                    command.AddSpecialIdentifier("hostviewercount", viewerCount.ToString());
                    await this.RunEventCommand(command, user);
                }
            }
            else if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
            {
                user.SubscribeDate = DateTimeOffset.Now;
                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                }

                if (this.OnSubscribedOccurred != null)
                {
                    this.OnSubscribedOccurred(this, user);
                }

                await this.RunEventCommand(this.FindMatchingEventCommand(e.channel), user);
            }
            else if (e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedEvent.ToString()) || e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedSharedEvent.ToString()))
            {
                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    user.Data.AddCurrencyAmount(currency, currency.OnSubscribeBonus);
                }

                int resubMonths = 0;
                if (e.payload.TryGetValue("totalMonths", out JToken resubMonthsToken))
                {
                    resubMonths = (int)resubMonthsToken;
                }

                if (this.OnResubscribedOccurred != null)
                {
                    this.OnResubscribedOccurred(this, new Tuple<UserViewModel, int>(user, resubMonths));
                }

                await this.RunEventCommand(this.FindMatchingEventCommand(ConstellationClientWrapper.ChannelResubscribedEvent.ToString()), user);
            }

            if (this.OnEventOccurred != null)
            {
                this.OnEventOccurred(this, e);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            UserViewModel user = new UserViewModel(0, donation.Username);

            UserModel userModel = await ChannelSession.Connection.GetUser(donation.Username);
            if (userModel != null)
            {
                user = new UserViewModel(userModel);
            }

            EventCommand command = this.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.Donation));
            if (command != null)
            {
                command.AddSpecialIdentifier("donationamount", donation.AmountText);
                command.AddSpecialIdentifier("donationmessage", donation.Message);
                await this.RunEventCommand(command, user);
            }
        }

        private EventCommand FindMatchingEventCommand(string eventDetails)
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

        private async Task RunEventCommand(EventCommand command, UserViewModel user)
        {
            if (command != null)
            {
                if (user != null)
                {
                    await command.Perform(user);
                }
                else
                {
                    await command.Perform();
                }
            }
        }

        private async void ConstellationClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            await reconnectionLock.WaitAsync();

            ChannelSession.DisconnectionOccurred("Constellation");

            do
            {
                ChannelSession.ReconnectionAttemptOccurred("Constellation");

                await this.Disconnect();

                await Task.Delay(2000);
            } while (!await this.Connect());

            ChannelSession.ReconnectionOccurred("Constellation");

            reconnectionLock.Release();
        }

        private void Client_OnPingDisconnectOcurred(object sender, EventArgs e)
        {
            this.ConstellationClient_OnDisconnectOccurred(this, WebSocketCloseStatus.NormalClosure);
        }
    }
}
