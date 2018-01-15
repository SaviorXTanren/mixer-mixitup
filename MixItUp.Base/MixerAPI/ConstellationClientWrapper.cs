using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ConstellationClientWrapper : MixerRequestWrapperBase
    {
        public static ConstellationEventType ResubscribeSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        private static readonly List<ConstellationEventTypeEnum> subscribedEvents = new List<ConstellationEventTypeEnum>()
        {
            ConstellationEventTypeEnum.channel__id__followed, ConstellationEventTypeEnum.channel__id__hosted, ConstellationEventTypeEnum.channel__id__subscribed,
            ConstellationEventTypeEnum.channel__id__resubscribed, ConstellationEventTypeEnum.channel__id__resubShared
        };

        public ConstellationClient Client { get; private set; }

        public ConstellationClientWrapper() { }

        public async Task<bool> Connect()
        {
            this.Client = await this.RunAsync(ConstellationClient.Create(ChannelSession.Connection.Connection));
            if (this.Client != null)
            {
                if (await this.RunAsync(this.Client.Connect()))
                {
                    this.Client.OnDisconnectOccurred += ConstellationClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
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

        public async Task SubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.SubscribeToEvents(events)); }

        public async Task UnsubscribeToEvents(IEnumerable<ConstellationEventType> events) { await this.RunAsync(this.Client.UnsubscribeToEvents(events)); }

        public async Task Disconnect()
        {
            if (this.Client != null)
            {
                this.Client.OnDisconnectOccurred -= ConstellationClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    this.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    this.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    this.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }
                this.Client.OnSubscribedEventOccurred -= ConstellationClient_OnSubscribedEventOccurred;

                await this.RunAsync(this.Client.Disconnect());
            }
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            JToken userToken;
            UserViewModel user = null;
            if (e.payload.TryGetValue("user", out userToken))
            {
                user = new UserViewModel(userToken.ToObject<UserModel>());
            }
            else if (e.payload.TryGetValue("hoster", out userToken))
            {
                ChannelModel channel = userToken.ToObject<ChannelModel>();
                user = new UserViewModel(channel.id, channel.token);
            }

            if (user != null)
            {
                UserDataViewModel userData = ChannelSession.Settings.UserData.GetValueIfExists(user.ID, new UserDataViewModel(user));

                if (e.channel.Equals(UserCurrencyViewModel.ChannelFollowEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.Enabled)
                        {
                            userData.SetCurrencyAmount(currency, currency.OnFollowBonus);
                        }
                    }
                }
                else if (e.channel.Equals(UserCurrencyViewModel.ChannelHostedEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.Enabled)
                        {
                            userData.SetCurrencyAmount(currency, currency.OnHostBonus);
                        }
                    }
                }
                else if (e.channel.Equals(UserCurrencyViewModel.ChannelSubscribedEvent.ToString()) || e.channel.Equals(UserCurrencyViewModel.ChannelResubscribedEvent.ToString()) ||
                    e.channel.Equals(UserCurrencyViewModel.ChannelResubscribedSharedEvent.ToString()))
                {
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.Enabled)
                        {
                            userData.SetCurrencyAmount(currency, currency.OnSubscribeBonus);
                        }
                    }
                }
            }

            foreach (EventCommand command in ChannelSession.Settings.EventCommands)
            {
                EventCommand foundCommand = null;

                if (command.MatchesEvent(e))
                {
                    foundCommand = command;
                }

                if (command.EventType == ConstellationEventTypeEnum.channel__id__subscribed && e.channel.Equals(ConstellationClientWrapper.ResubscribeSharedEvent.ToString()))
                {
                    foundCommand = command;
                }

                if (foundCommand != null)
                {
                    if (user != null)
                    {
                        await command.Perform(user);
                    }
                    else
                    {
                        await command.Perform();
                    }

                    return;
                }
            }
        }

        private async void ConstellationClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            do
            {
                await this.Disconnect();

                await Task.Delay(2000);
            } while (!await this.Connect());

            ChannelSession.ReconnectionOccurred();
        }
    }
}
