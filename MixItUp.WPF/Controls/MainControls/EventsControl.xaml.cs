using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainCommandControlBase
    {
        private static readonly ConstellationEventType resubscribeSharedEvent = new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id);

        private static readonly List<ConstellationEventTypeEnum> subscribedEvents = new List<ConstellationEventTypeEnum>()
        {
            ConstellationEventTypeEnum.channel__id__followed, ConstellationEventTypeEnum.channel__id__hosted, ConstellationEventTypeEnum.channel__id__subscribed,
            ConstellationEventTypeEnum.channel__id__resubscribed, ConstellationEventTypeEnum.channel__id__resubShared
        };

        public EventsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshEventControls();

            if (await ChannelSession.ConnectConstellation())
            {
                ChannelSession.Constellation.Client.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;
                await ChannelSession.Constellation.SubscribeToEvents(EventsControl.subscribedEvents.Select(e => new ConstellationEventType(e, ChannelSession.Channel.id)));
            }
        }

        private void RefreshEventControls()
        {
            this.OnFollowCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__followed);
            this.OnHostCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__hosted);
            this.OnSubscribeCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__subscribed);
            this.OnResubscribeCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__resubscribed);
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

                if (command.EventType == ConstellationEventTypeEnum.channel__id__subscribed && e.channel.Equals(resubscribeSharedEvent.ToString()))
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

        private async void GlobalEvents_OnCommandUpdated(object sender, CommandBase e)
        {
            if (e is EventCommand)
            {
                await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.RefreshEventControls();
            }
        }

        private void GlobalEvents_OnCommandDeleted(object sender, CommandBase e)
        {
            if (e is EventCommand)
            {
                ChannelSession.Settings.EventCommands.Remove((EventCommand)e);

                this.GlobalEvents_OnCommandUpdated(sender, e);
            }
        }
    }
}
