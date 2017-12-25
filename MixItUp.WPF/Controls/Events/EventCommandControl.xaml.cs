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

        public EventCommand Command { get; set; }

        public EventCommandItem(EventCommand command) : this(command.EventType) { this.Command = command; }

        public EventCommandItem(ConstellationEventTypeEnum eventType) { this.EventType = eventType; }

        public string Name { get { return EnumHelper.GetEnumName(this.EventType); } }
    }

    /// <summary>
    /// Interaction logic for EventCommandControl.xaml
    /// </summary>
    public partial class EventCommandControl : UserControl
    {
        private MainCommandControlBase mainControl;
        private ConstellationEventTypeEnum eventType;

        private EventCommandItem commandItem;

        public EventCommandControl() { InitializeComponent(); }

        public void Initialize(MainCommandControlBase control, ConstellationEventTypeEnum eventType)
        {
            this.mainControl = control;
            this.eventType = eventType;
            this.RefreshControl();
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new EventCommandDetailsControl(this.commandItem.EventType));
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CommandButtons_PlayClicked(object sender, RoutedEventArgs e)
        {
            await this.mainControl.HandleCommandPlay(sender);
        }

        private void CommandButtons_StopClicked(object sender, RoutedEventArgs e)
        {
            this.mainControl.HandleCommandStop(sender);
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            EventCommand command = this.mainControl.GetCommandFromCommandButtons<EventCommand>(sender);
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
                EventCommand command = this.mainControl.GetCommandFromCommandButtons<EventCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshControl();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.mainControl.HandleCommandEnableDisable(sender);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshControl();
        }

        private void RefreshControl()
        {
            EventCommand command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(this.eventType));
            this.commandItem = (command != null) ? new EventCommandItem(command) : new EventCommandItem(this.eventType);
            this.GroupBox.Header = this.commandItem.Name;

            this.CommandButtons.DataContext = this.commandItem.Command;

            this.NewInteractiveCommandButton.Visibility = (this.commandItem.Command == null) ? Visibility.Visible : Visibility.Collapsed;
            this.CommandButtons.Visibility = (this.commandItem.Command != null) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
