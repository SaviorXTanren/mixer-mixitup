using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandsControl.xaml
    /// </summary>
    public partial class CommandsControl : MainControlBase
    {
        private ObservableCollection<CommandBase> commands;

        public CommandsControl()
        {
            InitializeComponent();

            this.commands = new ObservableCollection<CommandBase>();
        }

        protected override Task InitializeInternal()
        {
            this.EditCommandButton.IsEnabled = false;
            this.DeleteCommandButton.IsEnabled = false;

            this.RefreshList();

            return Task.FromResult(0);
        }

        private void RefreshList()
        {
            if (MixerAPIHandler.Settings != null)
            {
                this.CommandsListView.ItemsSource = this.commands;
                this.commands.Clear();

                foreach (CommandBase command in MixerAPIHandler.Settings.ChatCommands)
                {
                    this.commands.Add(command);
                }

                foreach (CommandBase command in MixerAPIHandler.Settings.InteractiveCommands)
                {
                    this.commands.Add(command);
                }

                foreach (CommandBase command in MixerAPIHandler.Settings.EventCommands)
                {
                    this.commands.Add(command);
                }

                foreach (CommandBase command in MixerAPIHandler.Settings.TimerCommands)
                {
                    this.commands.Add(command);
                }
            }
        }

        private void CommandsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.EditCommandButton.IsEnabled = false;
            this.DeleteCommandButton.IsEnabled = false;
            if (this.CommandsListView.SelectedIndex >= 0)
            {
                this.EditCommandButton.IsEnabled = true;
                this.DeleteCommandButton.IsEnabled = true;
            }
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandDetailsWindow window = new CommandDetailsWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void EditCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CommandsListView.SelectedIndex >= 0)
            {
                CommandDetailsWindow window = new CommandDetailsWindow((CommandBase)this.CommandsListView.SelectedItem);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void DeleteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CommandsListView.SelectedIndex >= 0)
            {
                CommandBase command = (CommandBase)this.CommandsListView.SelectedItem;
                this.commands.Remove(command);

                MixerAPIHandler.Settings.ChatCommands.Remove((ChatCommand)command);
                MixerAPIHandler.Settings.InteractiveCommands.Remove((InteractiveCommand)command);
                MixerAPIHandler.Settings.EventCommands.Remove((EventCommand)command);
                MixerAPIHandler.Settings.TimerCommands.Remove((TimerCommand)command);

                this.CommandsListView.SelectedIndex = -1;

                this.RefreshList();
            }
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
            await MixerAPIHandler.SaveSettings();
        }
    }
}
