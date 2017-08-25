using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandDetailsWindow.xaml
    /// </summary>
    public partial class CommandDetailsWindow : LoadingWindowBase
    {
        public CommandBase Command;

        private CommandTypeEnum type;

        private InteractiveControlModel interactiveControl;
        private SubscribedEventViewModel subscribedEvent;

        private ObservableCollection<ActionControl> actionControls;

        public CommandDetailsWindow(CommandTypeEnum type) : this(type, null) { }

        public CommandDetailsWindow(CommandBase command) : this(command.Type, command) { }

        public CommandDetailsWindow(InteractiveControlModel interactiveControl, CommandBase command = null)
            : this(CommandTypeEnum.Interactive, command)
        {
            this.interactiveControl = interactiveControl;
        }

        public CommandDetailsWindow(SubscribedEventViewModel subscribedEvent, CommandBase command = null)
            : this(CommandTypeEnum.Event, command)
        {
            this.subscribedEvent = subscribedEvent;
        }

        private CommandDetailsWindow(CommandTypeEnum type, CommandBase command)
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.type = type;
            this.Command = command;
            this.actionControls = new ObservableCollection<ActionControl>();

            if (this.type == CommandTypeEnum.Interactive && this.interactiveControl == null)
            {
                throw new InvalidOperationException("Interactive commands must have an interactive control set");
            }
            if (this.type == CommandTypeEnum.Event && this.subscribedEvent == null)
            {
                throw new InvalidOperationException("Event commands must have a subscribed event set");
            }

            this.Loaded += CommandDetailsWindow_Loaded;
        }

        private void CommandDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.ActionsListView.ItemsSource = this.actionControls;

            if (this.type == CommandTypeEnum.Interactive)
            {
                this.InteractiveCommandEventTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveCommandEventType>();
            }

            switch (this.type)
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

            if (this.Command != null)
            {
                this.NameTextBox.Text = this.Command.Name;
                
                switch (this.Command.Type)
                {
                    case CommandTypeEnum.Chat:
                        ChatCommand chatCommand = (ChatCommand)this.Command;
                        this.ChatCommandTextBox.Text = chatCommand.Command;
                        break;
                    case CommandTypeEnum.Interactive:
                        InteractiveCommand interactiveCommand = (InteractiveCommand)this.Command;
                        this.InteractiveCommandEventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(interactiveCommand.EventType);
                        break;
                    case CommandTypeEnum.Event:
                        EventCommand eventCommand = (EventCommand)this.Command;
                        break;
                    case CommandTypeEnum.Timer:
                        TimerCommand timerCommand = (TimerCommand)this.Command;
                        this.TimerIntervalTextBox.Text = timerCommand.Interval.ToString();
                        this.TimerMinimumChatMessagesTextBox.Text = timerCommand.MinimumMessages.ToString();
                        break;
                }

                foreach (ActionBase action in this.Command.Actions)
                {
                    this.actionControls.Add(new ActionControl(action));
                }
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.actionControls.Add(new ActionControl());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(this.NameTextBox.Text))
            //{
            //    MessageBoxHelper.ShowError("Required command information is missing");
            //    return;
            //}

            //switch (this.type)
            //{
            //    case CommandTypeEnum.Chat:
            //        if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            //        {
            //            MessageBoxHelper.ShowError("Required chat command information is missing");
            //            return;
            //        }

            //        if (this.Command == null)
            //        {
            //            this.Command = new ChatCommand(this.NameTextBox.Text, this.ChatCommandTextBox.Text);
            //        }

            //        this.Command.Name = this.NameTextBox.Text;
            //        this.Command.Command = this.ChatCommandTextBox.Text;

            //        break;

            //    case CommandTypeEnum.Interactive:
            //        if (this.InteractiveCommandEventTypeComboBox.SelectedIndex < 0)
            //        {
            //            MessageBoxHelper.ShowError("Required interactive command information is missing");
            //            return;
            //        }

            //        InteractiveCommandEventType interactiveEventType = EnumHelper.GetEnumValueFromString<InteractiveCommandEventType>((string)this.InteractiveCommandEventTypeComboBox.SelectedItem);

            //        if (this.Command == null)
            //        {
            //            this.Command = new InteractiveCommand(this.NameTextBox.Text, this.interactiveControl.controlID, interactiveEventType);
            //            ChannelSession.Settings.InteractiveControls.Add((ChatCommand)this.Command);
            //        }

            //        this.Command.Name = this.NameTextBox.Text;

            //        break;

            //    case CommandTypeEnum.Event:
            //        newCommand = new EventCommand(this.NameTextBox.Text, this.subscribedEvent);
            //        break;

            //    case CommandTypeEnum.Timer:
            //        int timerInterval;
            //        int timerMinimumChatMessage;

            //        if (string.IsNullOrEmpty(this.TimerIntervalTextBox.Text) || !int.TryParse(this.TimerIntervalTextBox.Text, out timerInterval) || timerInterval <= 0 ||
            //            string.IsNullOrEmpty(this.TimerMinimumChatMessagesTextBox.Text) || !int.TryParse(this.TimerMinimumChatMessagesTextBox.Text, out timerMinimumChatMessage) || timerMinimumChatMessage <= 0)
            //        {
            //            MessageBoxHelper.ShowError("Required timer command information is missing");
            //            return;
            //        }

            //        newCommand = new TimerCommand(this.NameTextBox.Text, timerInterval, timerMinimumChatMessage);
            //        break;
            //}

            //if (this.actionControls.Count == 0)
            //{
            //    MessageBoxHelper.ShowError("At least one action must be created");
            //    return;
            //}

            //foreach (ActionControl control in this.actionControls)
            //{
            //    ActionBase action = control.GetAction(newCommand);
            //    if (action == null)
            //    {
            //        MessageBoxHelper.ShowError("Required action information is missing");
            //        return;
            //    }
            //    newCommand.AddAction(action);
            //}

            //if (this.Command != null)
            //{
            //    ChannelSession.Settings.RemoveCommand(this.Command);
            //}
            //ChannelSession.Settings.AddCommand(newCommand);

            //this.Close();
        }
    }
}
