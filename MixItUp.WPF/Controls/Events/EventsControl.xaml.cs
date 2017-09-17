using Mixer.Base.Clients;
using Mixer.Base.Model.Constellation;
using MixItUp.Base;
using MixItUp.Base.Commands;
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
                if (command.UniqueEventID.Equals(e.channel))
                {
                    await command.Perform();
                }
            }
        }
    }
}
