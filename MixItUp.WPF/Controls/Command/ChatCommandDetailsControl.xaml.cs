using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
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
            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.CooldownTextBox.Text = this.command.Cooldown.ToString();
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                this.Requirements.SetRequirements(this.command.Requirements);
            }
            else
            {
                this.CooldownTextBox.Text = "0";
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

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("Commands is missing");
                return false;
            }

            if (this.ChatCommandTextBox.Text.Any(c => !Char.IsLetterOrDigit(c) && !Char.IsWhiteSpace(c) && c != '!'))
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

            foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
            {
                if (this.GetExistingCommand() != command && this.NameTextBox.Text.Equals(command.Name))
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists a command with the same name");
                    return false;
                }
            }

            IEnumerable<string> commandStrings = this.GetCommandStrings();
            if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Each command string must be unique");
                return false;
            }

            foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
            {
                if (command.IsEnabled && this.GetExistingCommand() != command)
                {
                    if (commandStrings.Any(c => command.Commands.Contains(c)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists an enabled, chat command that uses one of the command strings you have specified");
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
                int cooldown = 0;
                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    cooldown = int.Parse(this.CooldownTextBox.Text);
                }

                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new ChatCommand(this.NameTextBox.Text, commands, cooldown, requirements);
                    ChannelSession.Settings.ChatCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                    this.command.Commands = commands.ToList();
                    this.command.Cooldown = cooldown;
                    this.command.Requirements = requirements;
                }
                return this.command;
            }
            return null;
        }

        private IEnumerable<string> GetCommandStrings() { return new List<string>(this.ChatCommandTextBox.Text.Replace("!", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)); }
    }
}
