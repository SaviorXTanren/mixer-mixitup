using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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

            this.CurrencyCostTextBox.IsEnabled = ChannelSession.Settings.CurrencyAcquisition.Enabled;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Permissions);
                this.CurrencyCostTextBox.Text = this.command.CurrencyCost.ToString();
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                this.CooldownTextBox.Text = this.command.Cooldown.ToString();
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

            if (this.LowestRoleAllowedComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A permission level must be selected");
                return false;
            }

            if (!string.IsNullOrEmpty(this.CurrencyCostTextBox.Text))
            {
                int cost = 0;
                if (!int.TryParse(this.CurrencyCostTextBox.Text, out cost) || cost < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Currency cost must be 0 or greater");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("Commands is missing");
                return false;
            }

            if (this.ChatCommandTextBox.Text.Any(c => !Char.IsLetterOrDigit(c) && !Char.IsWhiteSpace(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("Commands can only contain letters and numbers");
                return false;
            }

            if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
            {
                int cooldown = 0;
                if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                    return false;
                }
            }

            foreach (ChatCommand command in ChannelSession.AllChatCommands)
            {
                if (this.GetExistingCommand() != command && this.NameTextBox.Text.Equals(command.Name))
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists a chat command with the same name");
                    return false;
                }
            }

            IEnumerable<string> commandStrings = this.GetCommandStrings();
            if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Each command string must be unique");
                return false;
            }

            foreach (ChatCommand command in ChannelSession.AllChatCommands)
            {
                if (command.IsEnabled && this.GetExistingCommand() != command)
                {
                    if (commandStrings.Any(c => command.Commands.Contains(c)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a chat command that uses one of the command strings you have specified");
                        return false;
                    }
                }
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                IEnumerable<string> commands = this.GetCommandStrings();
                UserRole lowestRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.LowestRoleAllowedComboBox.SelectedItem);

                int cooldown = 0;
                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    cooldown = int.Parse(this.CooldownTextBox.Text);
                }

                int cost = 0;
                if (!string.IsNullOrEmpty(this.CurrencyCostTextBox.Text))
                {
                    cost = int.Parse(this.CurrencyCostTextBox.Text);
                }

                if (this.command == null)
                {
                    this.command = new ChatCommand(this.NameTextBox.Text, commands, lowestRole, cooldown, cost);
                    ChannelSession.Settings.ChatCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                    this.command.Commands = commands.ToList();
                    this.command.Permissions = lowestRole;
                    this.command.CurrencyCost = cost;
                    this.command.Cooldown = cooldown;
                }
                return this.command;
            }
            return null;
        }

        private IEnumerable<string> GetCommandStrings() { return new List<string>(this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)); }
    }
}
