using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class EventCommandItem
    {
        public ConstellationEventTypeEnum EventType { get; set; }
        public OtherEventTypeEnum OtherEventType { get; set; }

        public EventCommand Command { get; set; }

        public EventCommandItem(EventCommand command)
        {
            this.Command = command;
            if (this.Command.IsOtherEventType)
            {
                this.OtherEventType = this.Command.OtherEventType;
            }
            else
            {
                this.EventType = this.Command.EventType;
            }
        }

        public EventCommandItem(ConstellationEventTypeEnum eventType) { this.EventType = eventType; }

        public EventCommandItem(OtherEventTypeEnum otherEventType) { this.OtherEventType = otherEventType; }

        public string Name
        {
            get
            {
                if (this.OtherEventType != OtherEventTypeEnum.None)
                {
                    return EnumHelper.GetEnumName(this.OtherEventType);
                }
                return EnumHelper.GetEnumName(this.EventType);
            }
        }

        public string Service
        {
            get
            {
                if (this.OtherEventType == OtherEventTypeEnum.GameWispSubscribed || this.OtherEventType == OtherEventTypeEnum.GameWispResubscribed)
                {
                    return "GameWisp";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.GawkBoxDonation)
                {
                    return "GawkBox";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.StreamlabsDonation)
                {
                    return "Streamlabs";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.TiltifyDonation)
                {
                    return "Tiltify";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.ExtraLifeDonation)
                {
                    return "Extra Life";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.TipeeeStreamDonation)
                {
                    return "TipeeeStream";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.TreatStreamDonation)
                {
                    return "TreatStream";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.StreamJarDonation)
                {
                    return "StreamJar";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.TwitterStreamTweetRetweet)
                {
                    return "Twitter";
                }
                else if (this.OtherEventType == OtherEventTypeEnum.PatreonSubscribed)
                {
                    return "Patreon";
                }
                return "Mixer";
            }
        }

        public Visibility NewCommandButtonVisibility { get { return (this.Command == null) ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility ExistingCommandButtonsVisibility { get { return (this.Command != null) ? Visibility.Visible : Visibility.Collapsed; } }
    }

    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        private ObservableCollection<EventCommandItem> eventCommands = new ObservableCollection<EventCommandItem>();

        public EventsControl()
        {
            InitializeComponent();

            this.EventsCommandsDataGrid.ItemsSource = eventCommands;
        }

        protected override Task InitializeInternal()
        {
            this.RefreshControls();
            return Task.FromResult(0);
        }

        private void RefreshControls()
        {
            this.eventCommands.Clear();

            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerChannelStreamStart));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerChannelStreamStop));
            this.eventCommands.Add(this.GetEventCommand(ConstellationEventTypeEnum.channel__id__followed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerUserUnfollow));
            this.eventCommands.Add(this.GetEventCommand(ConstellationEventTypeEnum.channel__id__hosted));
            this.eventCommands.Add(this.GetEventCommand(ConstellationEventTypeEnum.channel__id__subscribed));
            this.eventCommands.Add(this.GetEventCommand(ConstellationEventTypeEnum.channel__id__resubscribed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerSparksUsed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerEmbersUsed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerSkillUsed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerMilestoneReached));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerUserFirstJoin));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerUserPurge));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerUserBan));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.MixerChatMessage));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.StreamlabsDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.GawkBoxDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.TiltifyDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.ExtraLifeDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.TipeeeStreamDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.TreatStreamDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.StreamJarDonation));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.TwitterStreamTweetRetweet));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.PatreonSubscribed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.GameWispSubscribed));
            this.eventCommands.Add(this.GetEventCommand(OtherEventTypeEnum.GameWispResubscribed));
        }

        private EventCommandItem GetEventCommand(ConstellationEventTypeEnum eventType)
        {
            EventCommand command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(eventType));

            if (command != null)
            {
                return new EventCommandItem(command);
            }
            else
            {
                return new EventCommandItem(eventType);
            }
        }

        private EventCommandItem GetEventCommand(OtherEventTypeEnum eventType)
        {
            EventCommand command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.OtherEventType.Equals(eventType));
            if (command != null)
            {
                return new EventCommandItem(command);
            }
            else
            {
                return new EventCommandItem(eventType);
            }
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            EventCommandItem eventCommand = (EventCommandItem)button.DataContext;
            CommandWindow window = new CommandWindow((eventCommand.OtherEventType != OtherEventTypeEnum.None) ?
                new EventCommandDetailsControl(eventCommand.OtherEventType) : new EventCommandDetailsControl(eventCommand.EventType));
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            EventCommand command = commandButtonsControl.GetCommandFromCommandButtons<EventCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new EventCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                EventCommand command = commandButtonsControl.GetCommandFromCommandButtons<EventCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshControls();
                }
            });
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshControls();
        }
    }
}
