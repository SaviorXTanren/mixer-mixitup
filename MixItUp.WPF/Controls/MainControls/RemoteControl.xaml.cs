using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for RemoteControl.xaml
    /// </summary>
    public partial class RemoteControl : MainControlBase
    {
        private ObservableCollection<RemoteCommand> remoteCommands = new ObservableCollection<RemoteCommand>();

        public RemoteControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.RemoteCommandsListView.ItemsSource = this.remoteCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.RemoteCommandsListView.SelectedIndex = -1;

            this.remoteCommands.Clear();
            foreach (RemoteCommand command in ChannelSession.Settings.RemoteCommands.OrderBy(c => c.Name))
            {
                this.remoteCommands.Add(command);
            }
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            RemoteCommand command = commandButtonsControl.GetCommandFromCommandButtons<RemoteCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new RemoteCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                RemoteCommand command = commandButtonsControl.GetCommandFromCommandButtons<RemoteCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.RemoteCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new RemoteCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void AddReferenceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                RemoteReferenceCommandDialogControl dialogControl = new RemoteReferenceCommandDialogControl();
                string result = await MessageBoxHelper.ShowCustomDialog(dialogControl);
                if (!string.IsNullOrEmpty(result) && result.Equals("Save") && dialogControl.ReferenceCommand != null)
                {
                    ChannelSession.Settings.RemoteCommands.Add(new RemoteCommand(dialogControl.ReferenceCommand.Name, dialogControl.ReferenceCommand));
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private async void GetCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string code = "1234";   // Add code to query web service for connection code

            // Normal Connection logic

            //this.RemoteCodeTextBlock.Text = "Code: " + code;
            //this.RemoteCodeTextBlock.Visibility = Visibility.Visible;
            //this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Visible;
            //this.GetCodeButton.Visibility = Visibility.Collapsed;

            //for (int i = 30; i > 0; i--)
            //{
            //    this.RemoteCodeTimeLeftTextBlock.Text = i + " Seconds Left...";
            //    await Task.Delay(1000);
            //}

            //this.RemoteCodeTextBlock.Visibility = Visibility.Collapsed;
            //this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Collapsed;
            //this.GetCodeButton.Visibility = Visibility.Visible;

            this.LogInGrid.Visibility = Visibility.Collapsed;
            this.RemoteSetupGrid.Visibility = Visibility.Visible;
        }

        private void DisconnectRemoteButton_Click(object sender, RoutedEventArgs e)
        {
            this.RemoteCodeTextBlock.Visibility = Visibility.Collapsed;
            this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Collapsed;
            this.GetCodeButton.Visibility = Visibility.Visible;

            this.LogInGrid.Visibility = Visibility.Visible;
            this.RemoteSetupGrid.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
