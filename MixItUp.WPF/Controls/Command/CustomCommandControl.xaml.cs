using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Command;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for CustomCommandControl.xaml
    /// </summary>
    public partial class CustomCommandControl : UserControl
    {
        private LoadingWindowBase window;

        private CustomCommand command;

        public CustomCommandControl() { InitializeComponent(); }

        public void Initialize(LoadingWindowBase window, CustomCommand command)
        {
            this.window = window;
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
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
            window.Show();
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = true;
        }

        private void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = false;
        }
    }
}
