using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for TimerCommandDetailsControl.xaml
    /// </summary>
    public partial class TimerCommandDetailsControl : CommandDetailsControlBase
    {
        private TimerCommand command;

        public TimerCommandDetailsControl(TimerCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public TimerCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            this.CommandGroupComboBox.ItemsSource = ChannelSession.Settings.CommandGroups.Keys;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.CommandGroupComboBox.Text = this.command.GroupName;
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.SetGroupTimerInterval();
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("Name is missing");
                return false;
            }

            if (!string.IsNullOrEmpty(this.GroupTimerTextBox.Text))
            {
                if (!string.IsNullOrEmpty(this.GroupTimerTextBox.Text) && (!int.TryParse(this.GroupTimerTextBox.Text, out int timerInterval) || timerInterval < 1))
                {
                    await MessageBoxHelper.ShowMessageDialog("All timer group intervals must be greater than 0 or left blank");
                    return false;
                }
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                if (this.command == null)
                {
                    this.command = new TimerCommand(this.NameTextBox.Text);
                    ChannelSession.Settings.TimerCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                }
                this.command.Unlocked = this.UnlockedControl.Unlocked;

                this.command.GroupName = this.CommandGroupComboBox.Text;
                if (!string.IsNullOrEmpty(this.CommandGroupComboBox.Text))
                {
                    if (!ChannelSession.Settings.CommandGroups.ContainsKey(this.CommandGroupComboBox.Text))
                    {
                        ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text] = new CommandGroupSettings(this.CommandGroupComboBox.Text);
                    }

                    ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text].Name = this.CommandGroupComboBox.Text;
                    if (!string.IsNullOrEmpty(this.GroupTimerTextBox.Text) && int.TryParse(this.GroupTimerTextBox.Text, out int timerInterval))
                    {
                        ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text].TimerInterval = timerInterval;
                    }
                    else
                    {
                        ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text].TimerInterval = 0;
                    }
                }

                return this.command;
            }
            return null;
        }

        private void CommandGroupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SetGroupTimerInterval();
        }

        private void CommandGroupComboBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.SetGroupTimerInterval();
        }

        private void SetGroupTimerInterval()
        {
            this.GroupTimerTextBox.IsEnabled = false;
            if (!string.IsNullOrEmpty(this.CommandGroupComboBox.Text))
            {
                this.GroupTimerTextBox.IsEnabled = true;
                if (ChannelSession.Settings.CommandGroups.ContainsKey(this.CommandGroupComboBox.Text))
                {
                    CommandGroupSettings settings = ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text];
                    if (settings.TimerInterval > 0)
                    {
                        this.GroupTimerTextBox.Text = settings.TimerInterval.ToString();
                    }
                }
            }
        }
    }
}
