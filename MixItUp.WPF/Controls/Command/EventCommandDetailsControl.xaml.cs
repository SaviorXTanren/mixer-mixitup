using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for EventCommandDetailsControl.xaml
    /// </summary>
    public partial class EventCommandDetailsControl : CommandDetailsControlBase
    {
        private EventCommand command;

        public EventCommandDetailsControl(EventCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public EventCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            this.EventTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConstellationEventTypeEnum>();

            if (this.command != null)
            {
                this.EventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.EventType);
                this.EventIDTextBox.Text = this.command.CommandsString;

                this.EventTypeComboBox.IsEnabled = false;
                this.EventIDTextBox.IsEnabled = false;
            }

            return Task.FromResult(0);
        }

        public override IEnumerable<ActionTypeEnum> GetAllowedActions() { return EventCommand.AllowedActions; }

        public override bool Validate()
        {
            if (this.EventTypeComboBox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowError("An event type must be selected");
                return false;
            }

            if (this.EventIDTextBox.IsEnabled && string.IsNullOrEmpty(this.EventIDTextBox.Text))
            {
                MessageBoxHelper.ShowError("A name must be specified for this event type");
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (this.Validate())
            {
                ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);

                ChannelAdvancedModel channel = null;
                UserModel user = null;

                if (eventType.ToString().Contains("channel"))
                {
                    channel = await ChannelSession.MixerConnection.Channels.GetChannel(this.EventIDTextBox.Text);
                    if (channel == null)
                    {
                        MessageBoxHelper.ShowError("Unable to find the channel for the specified username");
                        return null;
                    }
                }
                else if (eventType.ToString().Contains("user"))
                {
                    user = await ChannelSession.MixerConnection.Users.GetUser(this.EventIDTextBox.Text);
                    if (user == null)
                    {
                        MessageBoxHelper.ShowError("Unable to find a user for the specified username");
                        return null;
                    }
                }

                if (this.command == null)
                {
                    if (channel != null)
                    {
                        this.command = new EventCommand(eventType, channel);
                    }
                    else if (user != null)
                    {
                        this.command = new EventCommand(eventType, user);
                    }
                    else
                    {
                        this.command = new EventCommand(eventType);
                    }

                    if (ChannelSession.Settings.EventCommands.Any(se => se.ContainsCommand(this.command.CommandsString)))
                    {
                        MessageBoxHelper.ShowError("This event already exists");
                        return null;
                    }

                    ChannelSession.Settings.EventCommands.Add(this.command);
                }

                return this.command;
            }
            return null;
        }

        private void EventTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.EventTypeComboBox.SelectedIndex >= 0)
            {
                ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);
                if (eventType.ToString().Contains("id") && this.command == null)
                {
                    this.EventIDTextBox.IsEnabled = true;
                }
                else
                {
                    this.EventIDTextBox.IsEnabled = false;
                    this.EventIDTextBox.Clear();
                }
            }
        }
    }
}
