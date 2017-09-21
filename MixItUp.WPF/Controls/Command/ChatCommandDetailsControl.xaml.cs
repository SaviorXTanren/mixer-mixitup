using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for ChatCommandDetailsControl.xaml
    /// </summary>
    public partial class ChatCommandDetailsControl : CommandDetailsControlBase
    {
        private ChatCommand command;

        public ChatCommandDetailsControl(ChatCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public ChatCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            this.LowestRoleAllowedComboBox.ItemsSource = ChatCommand.PermissionsAllowedValues;
            this.LowestRoleAllowedComboBox.SelectedIndex = 0;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Permissions);
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                this.CooldownTextBox.Text = this.command.Cooldown.ToString();
            }

            return Task.FromResult(0);
        }

        public override IEnumerable<ActionTypeEnum> GetAllowedActions() { return ChatCommand.AllowedActions; }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                MessageBoxHelper.ShowDialog("Name is missing");
                return false;
            }

            if (this.LowestRoleAllowedComboBox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowDialog("A permission level must be selected");
                return false;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                MessageBoxHelper.ShowDialog("Commands is missing");
                return false;
            }

            int cooldown = 0;
            if (string.IsNullOrEmpty(this.CooldownTextBox.Text) || !int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
            {
                MessageBoxHelper.ShowDialog("Cooldown is missing");
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override Task<CommandBase> GetNewCommand()
        {
            if (this.Validate())
            {
                List<string> commands = new List<string>(this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                UserRole lowestRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.LowestRoleAllowedComboBox.SelectedItem);
                int cooldown = int.Parse(this.CooldownTextBox.Text);
                if (this.command == null)
                {
                    this.command = new ChatCommand(this.NameTextBox.Text, commands, lowestRole, cooldown);
                    ChannelSession.Settings.ChatCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                    this.command.Commands = commands;
                    this.command.Permissions = lowestRole;
                    this.command.Cooldown = cooldown;
                }
                return Task.FromResult<CommandBase>(this.command);
            }
            return Task.FromResult<CommandBase>(null);
        }
    }
}
