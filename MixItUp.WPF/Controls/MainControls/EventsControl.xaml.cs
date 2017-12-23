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
    public partial class EventsControl : MainControlBase
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

            await ChannelSession.SaveSettings();

            GlobalEvents.OnCommandUpdated += GlobalEvents_OnCommandUpdated;

            if (await ChannelSession.ConnectConstellation())
            {
                ChannelSession.Constellation.Client.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;
                await ChannelSession.Constellation.SubscribeToEvents(EventsControl.subscribedEvents.Select(e => new ConstellationEventType(e, ChannelSession.User.id)));
            }
        }

        private void RefreshEventControls()
        {
            this.OnFollowCommandControl.Initialize(this.Window, ConstellationEventTypeEnum.channel__id__followed);
            this.OnHostCommandControl.Initialize(this.Window, ConstellationEventTypeEnum.channel__id__hosted);
            this.OnSubscribeCommandControl.Initialize(this.Window, ConstellationEventTypeEnum.channel__id__subscribed);
            this.OnResubscribeCommandControl.Initialize(this.Window, ConstellationEventTypeEnum.channel__id__resubscribed);
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
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
                    GlobalEvents.EventOccurred(command.GetEventType());

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
    }
}
