using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ConstellationClientWrapper : MixerWebSocketWrapper
    {
        public static ConstellationEventType ChannelUpdateEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__update, ChannelSession.Channel.id); } }

        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribeSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        private static readonly List<ConstellationEventTypeEnum> subscribedEvents = new List<ConstellationEventTypeEnum>()
        {
            ConstellationEventTypeEnum.channel__id__followed, ConstellationEventTypeEnum.channel__id__hosted, ConstellationEventTypeEnum.channel__id__subscribed,
            ConstellationEventTypeEnum.channel__id__resubscribed, ConstellationEventTypeEnum.channel__id__resubShared, ConstellationEventTypeEnum.channel__id__update
        };

        public event EventHandler<ConstellationLiveEventModel> OnEventOccurred;

        public ConstellationClient Client { get; private set; }

        public ConstellationClientWrapper() { }

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
            }
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.Client = await this.RunAsync(ConstellationClient.Create(ChannelSession.Connection.Connection));
            if (this.Client != null)
            {
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

                    return true;
                }
            }
            return false;
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            ChannelModel channel = null;
            UserViewModel user = null;

            JToken userToken;
            if (e.payload.TryGetValue("user", out userToken))
            {
                user = new UserViewModel(userToken.ToObject<UserModel>());

                JToken subscribeStartToken;
                if (e.payload.TryGetValue("since", out subscribeStartToken))
                {
                    user.SubscribeDate = subscribeStartToken.ToObject<DateTimeOffset>();
                }
            }
            else if (e.payload.TryGetValue("hoster", out userToken))
            {
                channel = userToken.ToObject<ChannelModel>();
                user = new UserViewModel(channel.id, channel.token);
            }

            if (user != null)
            {
                UserDataViewModel userData = ChannelSession.Settings.UserData.GetValueIfExists(user.ID, new UserDataViewModel(user));

                if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        userData.SetCurrencyAmount(currency, currency.OnFollowBonus);
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelHostedEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        userData.SetCurrencyAmount(currency, currency.OnHostBonus);
                    }
                }
                else if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscribedEvent.ToString()) || e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedEvent.ToString()) ||
                    e.channel.Equals(ConstellationClientWrapper.ChannelResubscribedSharedEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        userData.SetCurrencyAmount(currency, currency.OnSubscribeBonus);
                    }
                }

                if (e.channel.Equals(ConstellationClientWrapper.ChannelSubscribedEvent.ToString()))
                {
                    user.SubscribeDate = DateTimeOffset.Now;
                }
            }

            if (e.channel.Equals(ConstellationClientWrapper.ChannelUpdateEvent.ToString()))
            {
                IDictionary<string, JToken> payloadValues = e.payload;
                if (payloadValues.ContainsKey("online") && (bool)payloadValues["online"])
                {
                    UptimeChatCommand.SetUptime(DateTimeOffset.Now);
                }
            }
            else
            {
                foreach (EventCommand command in ChannelSession.Settings.EventCommands)
                {
                    EventCommand foundCommand = null;

                    if (command.MatchesEvent(e))
                    {
                        foundCommand = command;
                    }

                    if (command.EventType == ConstellationEventTypeEnum.channel__id__subscribed && e.channel.Equals(ConstellationClientWrapper.ChannelResubscribeSharedEvent.ToString()))
                    {
                        foundCommand = command;
                    }

                    if (foundCommand != null)
                    {
                        if (command.EventType == ConstellationEventTypeEnum.channel__id__hosted && channel != null)
                        {
                            foundCommand.AddSpecialIdentifier("hostviewercount", channel.viewersCurrent.ToString());
                        }

                        if (user != null)
                        {
                            await foundCommand.Perform(user);
                        }
                        else
                        {
                            await foundCommand.Perform();
                        }

                        return;
                    }
                }
            }

            if (this.OnEventOccurred != null)
            {
                this.OnEventOccurred(this, e);
            }
        }

        private async void ConstellationClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Constellation");

            do
            {
                await this.Disconnect();

                await Task.Delay(2000);
            } while (!await this.Connect());

            ChannelSession.ReconnectionOccurred("Constellation");
        }
    }
}
