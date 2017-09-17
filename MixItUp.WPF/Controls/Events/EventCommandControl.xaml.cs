using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Events
{
    /// <summary>
    /// Interaction logic for EventCommandControl.xaml
    /// </summary>
    public partial class EventCommandControl : UserControl
    {
        private EventCommand command;

        public EventCommandControl()
        {
            InitializeComponent();
        }

        public void Initialize(EventCommand command)
        {
            this.DataContext = this.command = command;
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            this.TestButton.IsEnabled = false;
            await this.command.Perform();
            this.TestButton.IsEnabled = true;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new EventCommandDetailsControl(this.command));
            window.Show();
        }
    }
}
