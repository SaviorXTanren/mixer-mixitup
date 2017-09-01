using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Events
{
    /// <summary>
    /// Interaction logic for EventCommandWindow.xaml
    /// </summary>
    public partial class EventCommandWindow : LoadingWindowBase
    {
        private EventCommand command;

        private ObservableCollection<ActionControl> actionControls;

        private List<ActionTypeEnum> allowedActions = new List<ActionTypeEnum>()
        {
            ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram,
            ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
        };

        public EventCommandWindow()
            : this(null)
        {
            this.EventTypeComboBox.IsEnabled = true;
            this.EventIDTextBox.IsEnabled = true;
        }

        public EventCommandWindow(EventCommand command)
        {
            this.command = command;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.ActionsListView.ItemsSource = this.actionControls;

            this.EventTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConstellationEventTypeEnum>();

            if (this.command != null)
            {
                this.EventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.EventType);
                this.EventIDTextBox.Text = this.command.CommandsString;

                foreach (ActionBase action in this.command.Actions)
                {
                    this.actionControls.Add(new ActionControl(allowedActions, action));
                }
            }

            return base.OnLoaded();
        }

        private void EventTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.actionControls.Add(new ActionControl(allowedActions));
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.EventTypeComboBox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowError("An event type must be selected");
                return;
            }

            if (this.EventIDTextBox.IsEnabled && string.IsNullOrEmpty(this.EventIDTextBox.Text))
            {
                MessageBoxHelper.ShowError("A name must be specified for this event type");
                return;
            }

            List<ActionBase> newActions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowError("Required action information is missing");
                    return;
                }
                newActions.Add(action);
            }

            ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);

            await this.RunAsyncOperation(async () =>
            {
                ChannelAdvancedModel channel = null;
                UserModel user = null;

                if (eventType.ToString().Contains("channel"))
                {
                    channel = await MixerAPIHandler.MixerConnection.Channels.GetChannel(this.EventIDTextBox.Text);
                    if (channel == null)
                    {
                        MessageBoxHelper.ShowError("Unable to find the channel for the specified username");
                        return;
                    }
                }
                else if (eventType.ToString().Contains("user"))
                {
                    user = await MixerAPIHandler.MixerConnection.Users.GetUser(this.EventIDTextBox.Text);
                    if (user == null)
                    {
                        MessageBoxHelper.ShowError("Unable to find a user for the specified username");
                        return;
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
                        return;
                    }

                    ChannelSession.Settings.EventCommands.Add(this.command);
                }
            });

            this.command.Actions.Clear();
            this.command.Actions = newActions;

            this.Close();
        }
    }
}
