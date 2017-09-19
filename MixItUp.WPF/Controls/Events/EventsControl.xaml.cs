using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Events
{
    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        public EventsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            EventCommand followCommand = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(ConstellationEventTypeEnum.channel__id__followed));
            if (followCommand == null)
            {
                followCommand = new EventCommand(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel);
                ChannelSession.Settings.EventCommands.Add(followCommand);
            }
            this.OnFollowCommandControl.Initialize(followCommand);

            EventCommand hostCommand = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(ConstellationEventTypeEnum.channel__id__hosted));
            if (hostCommand == null)
            {
                hostCommand = new EventCommand(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel);
                ChannelSession.Settings.EventCommands.Add(hostCommand);
            }
            this.OnHostCommandControl.Initialize(hostCommand);

            EventCommand subscribeCommand = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(ConstellationEventTypeEnum.channel__id__subscribed));
            if (subscribeCommand == null)
            {
                subscribeCommand = new EventCommand(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel);
                ChannelSession.Settings.EventCommands.Add(subscribeCommand);
            }
            this.OnSubscribeCommandControl.Initialize(subscribeCommand);

            EventCommand resubscribeCommand = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(ConstellationEventTypeEnum.channel__id__resubscribed));
            if (resubscribeCommand == null)
            {
                resubscribeCommand = new EventCommand(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel);
                ChannelSession.Settings.EventCommands.Add(resubscribeCommand);
            }
            this.OnResubscribeCommandControl.Initialize(resubscribeCommand);

            EventCommand resubscribeSharedCommand = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(ConstellationEventTypeEnum.channel__id__resubShared));
            if (resubscribeSharedCommand == null)
            {
                resubscribeSharedCommand = new EventCommand(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel);
                ChannelSession.Settings.EventCommands.Add(resubscribeSharedCommand);
            }
            this.OnResubscribeSharedCommandControl.Initialize(resubscribeSharedCommand);

            await ChannelSession.Settings.Save();

            if (await ChannelSession.InitializeConstellationClient())
            {
                ChannelSession.ConstellationClient.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;
            }

            await ChannelSession.ConstellationClient.SubscribeToEvents(ChannelSession.Settings.EventCommands.Select(c => c.GetEventType()));
        }

        private async void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            foreach (EventCommand command in ChannelSession.Settings.EventCommands)
            {
                if (command.MatchesEvent(e))
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
                        await command.Perform(user);
                    }
                    else
                    {
                        await command.Perform();
                    }
                }
            }
        }
    }
}
