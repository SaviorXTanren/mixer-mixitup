using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Events
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
    }

    /// <summary>
    /// Interaction logic for EventCommandControl.xaml
    /// </summary>
    public partial class EventCommandControl : UserControl
    {
        private MainControlBase mainControl;
        private ConstellationEventTypeEnum eventType;
        OtherEventTypeEnum otherEventType;

        private EventCommandItem commandItem;

        public EventCommandControl() { InitializeComponent(); }

        public void Initialize(MainControlBase control, ConstellationEventTypeEnum eventType)
        {
            this.mainControl = control;
            this.eventType = eventType;
            this.RefreshControl();
        }

        public void Initialize(MainControlBase control, OtherEventTypeEnum otherEventType)
        {
            this.mainControl = control;
            this.otherEventType = otherEventType;
            this.RefreshControl();
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow((this.commandItem.OtherEventType != OtherEventTypeEnum.None) ?
                new EventCommandDetailsControl(this.commandItem.OtherEventType) : new EventCommandDetailsControl(this.commandItem.EventType));
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
            await this.mainControl.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                EventCommand command = commandButtonsControl.GetCommandFromCommandButtons<EventCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshControl();
                }
            });
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshControl();
        }

        private void RefreshControl()
        {
            EventCommand command = null;
            if (this.otherEventType != OtherEventTypeEnum.None)
            {
                command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.OtherEventType.Equals(this.otherEventType));
            }
            else
            {
                command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(this.eventType));
            }

            if (command != null)
            {
                this.commandItem = new EventCommandItem(command);
            }
            else
            {
                if (this.otherEventType != OtherEventTypeEnum.None)
                {
                    this.commandItem = new EventCommandItem(this.otherEventType);
                }
                else
                {
                    this.commandItem = new EventCommandItem(this.eventType);
                }
            }

            this.GroupBox.Header = this.commandItem.Name;
            this.CommandButtons.DataContext = this.commandItem.Command;

            this.NewInteractiveCommandButton.Visibility = (this.commandItem.Command == null) ? Visibility.Visible : Visibility.Collapsed;
            this.CommandButtons.Visibility = (this.commandItem.Command != null) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
