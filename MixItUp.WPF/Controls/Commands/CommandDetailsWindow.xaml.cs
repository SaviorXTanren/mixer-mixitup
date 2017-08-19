using Mixer.Base.Clients;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandDetailsWindow.xaml
    /// </summary>
    public partial class CommandDetailsWindow : Window
    {
        private InteractiveVersionModel selectedGameVersion;
        private List<InteractiveControlModel> interactiveControls;

        private CommandBase command;

        private ObservableCollection<ActionControl> actionControls;

        public CommandDetailsWindow(InteractiveVersionModel selectedGameVersion) : this(selectedGameVersion, null) { }

        public CommandDetailsWindow(InteractiveVersionModel selectedGameVersion, CommandBase command)
        {
            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.selectedGameVersion = selectedGameVersion;
            this.command = command;

            this.Loaded += CommandDetailsWindow_Loaded;
        }

        private void CommandDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> typeOptions = EnumHelper.GetEnumNames<CommandTypeEnum>().ToList();
            if (this.selectedGameVersion == null)
            {
                typeOptions.Remove(EnumHelper.GetEnumName(CommandTypeEnum.Interactive));
            }
            this.TypeComboBox.ItemsSource = typeOptions;

            this.ActionsListView.ItemsSource = this.actionControls;

            if (this.selectedGameVersion != null)
            {
                this.InteractiveCommandComboBox.ItemsSource = this.interactiveControls = this.selectedGameVersion.controls.scenes.SelectMany(s => s.allControls).OrderBy(c => c.controlID).ToList();
                this.InteractiveCommandEventTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveCommandEventType>();
            }

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Type);
                
                switch (this.command.Type)
                {
                    case CommandTypeEnum.Chat:
                        ChatCommand chatCommand = (ChatCommand)this.command;
                        this.ChatCommandTextBox.Text = chatCommand.Command;
                        this.ChatDescriptionTextBox.Text = chatCommand.Description;
                        break;
                    case CommandTypeEnum.Interactive:
                        InteractiveCommand interactiveCommand = (InteractiveCommand)this.command;
                        this.InteractiveCommandComboBox.SelectedItem = this.interactiveControls.FirstOrDefault(c => c.controlID.Equals(interactiveCommand.Command));
                        this.InteractiveCommandEventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(interactiveCommand.EventType);
                        break;
                    case CommandTypeEnum.Event:
                        EventCommand eventCommand = (EventCommand)this.command;
                        this.EventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(eventCommand.EventType);
                        break;
                    case CommandTypeEnum.Timer:
                        TimerCommand timerCommand = (TimerCommand)this.command;
                        this.TimerIntervalTextBox.Text = timerCommand.Interval.ToString();
                        this.TimerMinimumChatMessagesTextBox.Text = timerCommand.MinimumMessages.ToString();
                        break;
                }

                foreach (ActionBase action in this.command.Actions)
                {
                    this.actionControls.Add(new ActionControl(action));
                }
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ChatCommandGrid.Visibility = Visibility.Collapsed;
            this.InteractiveCommandGrid.Visibility = Visibility.Collapsed;
            this.EventCommandGrid.Visibility = Visibility.Collapsed;
            this.TimerCommandGrid.Visibility = Visibility.Collapsed;
            this.ActionsListView.Visibility = Visibility.Collapsed;

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
                MessageBoxHelper.ShowError("Required command information is missing");
                return;
            }

            string typeName = (string)this.TypeComboBox.SelectedItem;
            CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeName);

            if (this.actionControls.Count == 0)
            {
                MessageBoxHelper.ShowError("At least one action must be created");
                return;
            }

            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowError("Required action information is missing");
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
                        MessageBoxHelper.ShowError("Required chat command information is missing");
                        return;
                    }

                    newCommand = new ChatCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, this.ChatDescriptionTextBox.Text);
                    break;

                case CommandTypeEnum.Interactive:
                    if (this.InteractiveCommandComboBox.SelectedIndex < 0 || this.InteractiveCommandEventTypeComboBox.SelectedIndex < 0)
                    {
                        MessageBoxHelper.ShowError("Required interactive command information is missing");
                        return;
                    }

                    InteractiveControlModel interactiveControl = (InteractiveControlModel)this.InteractiveCommandComboBox.SelectedItem;
                    InteractiveCommandEventType interactiveEventType = EnumHelper.GetEnumValueFromString<InteractiveCommandEventType>((string)this.InteractiveCommandEventTypeComboBox.SelectedItem);

                    newCommand = new InteractiveCommand(this.NameTextBox.Text, interactiveControl.controlID, interactiveEventType, actions);
                    break;

                case CommandTypeEnum.Event:
                    if (this.EventTypeComboBox.SelectedIndex < 0)
                    {
                        MessageBoxHelper.ShowError("Required event command information is missing");
                        return;
                    }

                    string eventTypeName = (string)this.EventTypeComboBox.SelectedItem;
                    ConstellationEventTypeEnum constellationEventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>(eventTypeName);

                    newCommand = new EventCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, constellationEventType);
                    break;

                case CommandTypeEnum.Timer:
                    int timerInterval;
                    int timerMinimumChatMessage;

                    if (string.IsNullOrEmpty(this.TimerIntervalTextBox.Text) || !int.TryParse(this.TimerIntervalTextBox.Text, out timerInterval) || timerInterval <= 0 ||
                        string.IsNullOrEmpty(this.TimerMinimumChatMessagesTextBox.Text) || !int.TryParse(this.TimerMinimumChatMessagesTextBox.Text, out timerMinimumChatMessage) || timerMinimumChatMessage <= 0)
                    {
                        MessageBoxHelper.ShowError("Required chat command information is missing");
                        return;
                    }

                    newCommand = new TimerCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text, actions, timerInterval, timerMinimumChatMessage);
                    break;
            }

            if (newCommand != null)
            {
                if (this.command != null)
                {
                    if (newCommand is ChatCommand) { MixerAPIHandler.Settings.ChatCommands.Remove((ChatCommand)this.command); }
                    else if (newCommand is InteractiveCommand) { MixerAPIHandler.Settings.InteractiveCommands.Remove((InteractiveCommand)this.command); }
                    else if (newCommand is EventCommand) { MixerAPIHandler.Settings.EventCommands.Remove((EventCommand)this.command); }
                    else if (newCommand is TimerCommand) { MixerAPIHandler.Settings.TimerCommands.Remove((TimerCommand)this.command); }
                }

                if (newCommand is ChatCommand) { MixerAPIHandler.Settings.ChatCommands.Add((ChatCommand)newCommand); }
                else if (newCommand is InteractiveCommand) { MixerAPIHandler.Settings.InteractiveCommands.Add((InteractiveCommand)newCommand); }
                else if (newCommand is EventCommand) { MixerAPIHandler.Settings.EventCommands.Add((EventCommand)newCommand); }
                else if (newCommand is TimerCommand) { MixerAPIHandler.Settings.TimerCommands.Add((TimerCommand)newCommand); }
            }
            else
            {
                MessageBoxHelper.ShowError("Unknown error occurred");
                return;
            }

            this.Close();
        }
    }
}
