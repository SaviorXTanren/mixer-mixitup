using MixItUp.Base;
using MixItUp.Base.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for InteractiveJoystickSetupTestDialogControl.xaml
    /// </summary>
    public partial class InteractiveJoystickSetupTestDialogControl : UserControl
    {
        private InteractiveJoystickCommand command;

        public InteractiveJoystickSetupTestDialogControl(InteractiveJoystickCommand command)
        {
            this.command = command;

            InitializeComponent();

            this.Loaded += InteractiveJoystickSetupTestDialogControl_Loaded;
        }

        private async void InteractiveJoystickSetupTestDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            for (int i = 5; i > 0; i--)
            {
                this.StatusTextBlock.Text = "Testing joystick in:" + Environment.NewLine + Environment.NewLine + i.ToString();
                await Task.Delay(1000);
            }

            this.StatusTextBlock.Text = "Testing Joystick:" + Environment.NewLine + Environment.NewLine + "Up";
            await this.command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "0.0", "-1.0" });
            await Task.Delay(2500);

            this.StatusTextBlock.Text = "Testing Joystick:" + Environment.NewLine + Environment.NewLine + "Down";
            await this.command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "0.0", "1.0" });
            await Task.Delay(2500);

            this.StatusTextBlock.Text = "Testing Joystick:" + Environment.NewLine + Environment.NewLine + "Left";
            await this.command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "-1.0", "0.0" });
            await Task.Delay(2500);

            this.StatusTextBlock.Text = "Testing Joystick:" + Environment.NewLine + Environment.NewLine + "Right";
            await this.command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "1.0", "0.0" });
            await Task.Delay(2500);

            this.StatusTextBlock.Text = "Testing Complete";
            await this.command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "0.0", "0.0" });
        }
    }
}
