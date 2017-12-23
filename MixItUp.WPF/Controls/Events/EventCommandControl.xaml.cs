using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows;
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
        private LoadingWindowBase window;

        private EventCommandItem commandItem;

        public EventCommandControl() { InitializeComponent(); }

        public void Initialize(LoadingWindowBase window, ConstellationEventTypeEnum eventType)
        {
            this.window = window;

            EventCommand command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.EventType.Equals(eventType));
            this.commandItem = (command != null) ? new EventCommandItem(command) : new EventCommandItem(eventType);
            this.DataContext = this.commandItem;

            this.CommandButtons.Initialize(this.commandItem.Command);

            this.NewInteractiveCommandButton.Visibility = (this.commandItem.Command == null) ? Visibility.Visible : Visibility.Collapsed;
            this.CommandButtons.Visibility = (this.commandItem.Command != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new EventCommandDetailsControl(this.commandItem.EventType));
            window.Show();
        }
    }
}
