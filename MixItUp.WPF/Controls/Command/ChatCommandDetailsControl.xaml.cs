using MaterialDesignThemes.Wpf;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
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
        private const string ChatTriggersNoExclamationHintAssist = "Trigger(s) in Chat (EX: \"!follow\") (No \"!\" needed, space seperated)";
        private const string ChatTriggersHintAssist = "Trigger(s) in Chat (EX: \"!follow\") (Space seperated)";

        private ChatCommand command;

        private bool autoAddToChatCommands = true;

        public ChatCommandDetailsControl(ChatCommand command)
        {
            this.command = command;

            InitializeComponent();
        }

        public ChatCommandDetailsControl(bool autoAddToChatCommands = true) : this(null) { this.autoAddToChatCommands = autoAddToChatCommands; }

        public override Task Initialize()
        {
            this.IncludeExclamationInCommandsToggleButton.IsChecked = true;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                this.IncludeExclamationInCommandsToggleButton.IsChecked = this.command.IncludeExclamationInCommands;
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.Requirements.SetRequirements(this.command.Requirements);
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

            if (!CommandBase.IsValidCommandString(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("Triggers contain an invalid character");
                return false;
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

                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null || !ChannelSession.Settings.ChatCommands.Contains(this.command))
                {
                    this.command = new ChatCommand(this.NameTextBox.Text, commands, requirements);
                    if (this.autoAddToChatCommands)
                    {
                        ChannelSession.Settings.ChatCommands.Add(this.command);
                    }
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                    this.command.Commands = commands.ToList();
                    this.command.Requirements = requirements;
                }

                this.command.IncludeExclamationInCommands = this.IncludeExclamationInCommandsToggleButton.IsChecked.GetValueOrDefault();
                this.command.Unlocked = this.UnlockedControl.Unlocked;

                return this.command;
            }
            return null;
        }

        private IEnumerable<string> GetCommandStrings()
        {
            string commandStrings = this.ChatCommandTextBox.Text;
            if (this.IncludeExclamationInCommandsToggleButton.IsChecked.GetValueOrDefault())
            {
                commandStrings = commandStrings.Replace("!", "");
            }
            return new List<string>(commandStrings.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void IncludeExclamationInCommandsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.IncludeExclamationInCommandsToggleButton.IsChecked.GetValueOrDefault())
            {
                HintAssist.SetHint(this.ChatCommandTextBox, ChatTriggersNoExclamationHintAssist);
            }
            else
            {
                HintAssist.SetHint(this.ChatCommandTextBox, ChatTriggersHintAssist);
            }
        }
    }
}
