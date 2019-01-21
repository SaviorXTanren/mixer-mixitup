using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicChatCommandEditorControl.xaml
    /// </summary>
    public partial class BasicChatCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private ChatCommand command;
        private bool autoAddToChatCommands = false;

        private ActionControlBase actionControl;

        public BasicChatCommandEditorControl(CommandWindow window, ChatCommand command, bool autoAddToChatCommands = true)
            : this(window, BasicCommandTypeEnum.None, autoAddToChatCommands)
        {
            this.command = command;
        }

        public BasicChatCommandEditorControl(CommandWindow window, BasicCommandTypeEnum commandType, bool autoAddToChatCommands = true)
        {
            this.window = window;
            this.commandType = commandType;
            this.autoAddToChatCommands = autoAddToChatCommands;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.LowestRoleAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;

            if (this.command != null)
            {
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Requirements.Role.MixerRole);
                this.CooldownTextBox.Text = this.command.Requirements.Cooldown.Amount.ToString();
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                if (this.command.Actions.First() is ChatAction)
                {
                    this.actionControl = new ChatActionControl(null, (ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl(null, (SoundAction)this.command.Actions.First());
                }
            }
            else
            {
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(MixerRoleEnum.User);
                this.CooldownTextBox.Text = "0";
                if (this.commandType == BasicCommandTypeEnum.Chat)
                {
                    this.actionControl = new ChatActionControl(null);
                }
                else if (this.commandType == BasicCommandTypeEnum.Sound)
                {
                    this.actionControl = new SoundActionControl(null);
                }
            }

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (this.LowestRoleAllowedComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A permission level must be selected");
                    return;
                }

                int cooldown = 0;
                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Commands is missing");
                    return;
                }

                if (!CommandBase.IsValidCommandString(this.ChatCommandTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Triggers contain an invalid character");
                    return;
                }

                IEnumerable<string> commandStrings = new List<string>(this.ChatCommandTextBox.Text.Replace("!", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                if (commandStrings.Count() == 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("At least 1 chat trigger must be specified");
                    return;
                }

                if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Each chat trigger must be unique");
                    return;
                }

                foreach (PermissionsCommandBase command in ChannelSession.AllEnabledChatCommands)
                {
                    if (command.IsEnabled && this.GetExistingCommand() != command)
                    {
                        if (commandStrings.Any(c => command.Commands.Contains(c, StringComparer.InvariantCultureIgnoreCase)))
                        {
                            await MessageBoxHelper.ShowMessageDialog("There already exists an enabled, chat command that uses one of the command strings you have specified");
                            return;
                        }
                    }
                }

                ActionBase action = this.actionControl.GetAction();
                if (action == null)
                {
                    if (this.actionControl is ChatActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The chat message must not be empty");
                    }
                    else if (this.actionControl is SoundActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The sound file path must not be empty");
                    }
                    return;
                }

                RequirementViewModel requirement = new RequirementViewModel(EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.LowestRoleAllowedComboBox.SelectedItem), cooldown: cooldown);

                ChatCommand newCommand = new ChatCommand(commandStrings.First(), commandStrings, requirement);
                newCommand.IsBasic = true;
                newCommand.Actions.Add(action);

                if (this.autoAddToChatCommands)
                {
                    if (this.command != null)
                    {
                        ChannelSession.Settings.ChatCommands.Remove(this.command);
                    }
                    ChannelSession.Settings.ChatCommands.Add(newCommand);
                }

                this.CommandSavedSuccessfully(newCommand);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
