using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for ActionGroupCommandDetailsControl.xaml
    /// </summary>
    public partial class ActionGroupCommandDetailsControl : CommandDetailsControlBase
    {
        private static readonly string RunOneRandomlyTooltip = "This option will randomly perform one AND ONLY ONE of" +
            Environment.NewLine + "the actions below when this Action Group is performed";

        private ActionGroupCommand command;

        public ActionGroupCommandDetailsControl(ActionGroupCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public ActionGroupCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            this.RunOneRandomlyTextBlock.ToolTip = this.RunOneRandomlyToggleButton.ToolTip = RunOneRandomlyTooltip;
            this.CommandGroupComboBox.ItemsSource = ChannelSession.Settings.CommandGroups.Keys;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.CommandGroupComboBox.Text = this.command.GroupName;
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.RunOneRandomlyToggleButton.IsChecked = this.command.IsRandomized;
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await DialogHelper.ShowMessage("Name is missing");
                return false;
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
                    this.command = new ActionGroupCommand(this.NameTextBox.Text);
                    ChannelSession.Settings.ActionGroupCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                }
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.IsRandomized = this.RunOneRandomlyToggleButton.IsChecked.GetValueOrDefault();

                this.command.GroupName = !string.IsNullOrEmpty(this.CommandGroupComboBox.Text) ? this.CommandGroupComboBox.Text : null;
                if (!string.IsNullOrEmpty(this.CommandGroupComboBox.Text))
                {
                    if (!ChannelSession.Settings.CommandGroups.ContainsKey(this.CommandGroupComboBox.Text))
                    {
                        ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text] = new CommandGroupSettings(this.CommandGroupComboBox.Text);
                    }
                    ChannelSession.Settings.CommandGroups[this.CommandGroupComboBox.Text].Name = this.CommandGroupComboBox.Text;
                }

                return this.command;
            }
            return null;
        }
    }
}
