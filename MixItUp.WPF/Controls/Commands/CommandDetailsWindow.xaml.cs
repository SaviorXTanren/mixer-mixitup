using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandDetailsWindow.xaml
    /// </summary>
    public partial class CommandDetailsWindow : Window
    {
        public CommandBase Command { get; private set; }

        private ObservableCollection<ActionControl> actionControls;

        public CommandDetailsWindow() : this(null) { }

        public CommandDetailsWindow(CommandBase command)
        {
            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Command = command;

            this.Loaded += CommandDetailsWindow_Loaded;
        }

        private void CommandDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CommandTypeEnum>();

            this.ActionsListView.ItemsSource = this.actionControls;
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ChatCommandGrid.Visibility = Visibility.Hidden;
            this.InteractiveCommandGrid.Visibility = Visibility.Hidden;
            this.EventCommandGrid.Visibility = Visibility.Hidden;
            this.TimerCommandGrid.Visibility = Visibility.Hidden;
            this.ActionsListView.Visibility = Visibility.Hidden;

            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                string typeName = (string)this.TypeComboBox.SelectedItem;
                CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeName);

                switch (type)
                {
                    case CommandTypeEnum.Chat:
                        this.ChatCommandGrid.Visibility = Visibility.Visible;
                        break;
                    case CommandTypeEnum.Interactive:
                        this.InteractiveCommandGrid.Visibility = Visibility.Visible;
                        break;
                    case CommandTypeEnum.Event:
                        this.EventCommandGrid.Visibility = Visibility.Visible;
                        break;
                    case CommandTypeEnum.Timer:
                        this.TimerCommandGrid.Visibility = Visibility.Visible;
                        break;
                }
                
                this.ActionsListView.Visibility = Visibility.Visible;

                this.AddActionButton.IsEnabled = true;
                this.SaveButton.IsEnabled = true;
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.actionControls.Add(new ActionControl());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text) || this.TypeComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Required command information is missing");
                return;
            }

            string typeName = (string)this.TypeComboBox.SelectedItem;
            CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeName);

            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBox.Show("Required action information is missing");
                    return;
                }
                actions.Add(action);
            }

            CommandBase newCommand = null;
            switch (type)
            {
                case CommandTypeEnum.Chat:
                    if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text) || string.IsNullOrEmpty(this.ChatDescriptionTextBox.Text))
                    {
                        MessageBox.Show("Required chat command information is missing");
                        return;
                    }

                    newCommand = new ChatCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, this.ChatDescriptionTextBox.Text);
                    break;
                case CommandTypeEnum.Interactive:
                    newCommand = new InteractiveCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions);
                    break;
                case CommandTypeEnum.Event:
                    if (this.EventTypeComboBox.SelectedIndex < 0)
                    {
                        MessageBox.Show("Required event command information is missing");
                        return;
                    }

                    string eventTypeName = (string)this.EventTypeComboBox.SelectedItem;
                    ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>(eventTypeName);

                    newCommand = new EventCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, eventType);
                    break;
                case CommandTypeEnum.Timer:
                    int timerInterval;
                    int timerMinimumChatMessage;

                    if (string.IsNullOrEmpty(this.TimerIntervalTextBox.Text) || !int.TryParse(this.TimerIntervalTextBox.Text, out timerInterval) || timerInterval <= 0 ||
                        string.IsNullOrEmpty(this.TimerMinimumChatMessagesTextBox.Text) || !int.TryParse(this.TimerMinimumChatMessagesTextBox.Text, out timerMinimumChatMessage) || timerMinimumChatMessage <= 0)
                    {
                        MessageBox.Show("Required chat command information is missing");
                        return;
                    }

                    newCommand = new TimerCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, timerInterval, timerMinimumChatMessage);
                    break;
            }

            if (newCommand != null)
            {
                MixerAPIHandler.ChannelSettings.ChatCommands.Remove((ChatCommand)this.Command);
                MixerAPIHandler.ChannelSettings.InteractiveCommands.Remove((InteractiveCommand)this.Command);
                MixerAPIHandler.ChannelSettings.EventCommands.Remove((EventCommand)this.Command);
                MixerAPIHandler.ChannelSettings.TimerCommands.Remove((TimerCommand)this.Command);

                if (newCommand is ChatCommand) { MixerAPIHandler.ChannelSettings.ChatCommands.Add((ChatCommand)newCommand); }
                else if (newCommand is InteractiveCommand) { MixerAPIHandler.ChannelSettings.InteractiveCommands.Add((InteractiveCommand)newCommand); }
                else if (newCommand is EventCommand) { MixerAPIHandler.ChannelSettings.EventCommands.Add((EventCommand)newCommand); }
                else if (newCommand is TimerCommand) { MixerAPIHandler.ChannelSettings.TimerCommands.Add((TimerCommand)newCommand); }
            }
            else
            {
                MessageBox.Show("Unknown error occurred");
                return;
            }

            this.Close();
        }
    }
}
