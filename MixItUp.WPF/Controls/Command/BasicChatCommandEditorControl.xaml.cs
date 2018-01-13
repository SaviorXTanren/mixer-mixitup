using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
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

        private ChatActionControl chatActionControl;

        private BasicChatCommand command = null;

        public BasicChatCommandEditorControl(CommandWindow window, BasicChatCommand command)
            : this(window)
        {
            this.command = command;
        }

        public BasicChatCommandEditorControl(CommandWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.LowestRoleAllowedComboBox.ItemsSource = ChatCommand.PermissionsAllowedValues;

            if (this.command != null)
            {
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(command.Permissions);
                this.CooldownTextBox.Text = command.Cooldown.ToString();
                this.ChatCommandTextBox.Text = command.CommandsString;
                this.chatActionControl = new ChatActionControl(null, (ChatAction)this.command.Actions.First());
            }
            else
            {
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(UserRole.User);
                this.CooldownTextBox.Text = "0";
                this.chatActionControl = new ChatActionControl(null);
            }

            this.ChatActionControlControl.Content = this.chatActionControl;

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

                ActionBase action = this.chatActionControl.GetAction();
                if (action == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("The chat message must not be empty");
                    return;
                }

                if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Commands is missing");
                    return;
                }

                if (this.ChatCommandTextBox.Text.Any(c => !Char.IsLetterOrDigit(c) && !Char.IsWhiteSpace(c)))
                {
                    await MessageBoxHelper.ShowMessageDialog("Commands can only contain letters and numbers");
                    return;
                }

                IEnumerable<string> commandStrings = new List<string>(this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Each command string must be unique");
                    return;
                }

                foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
                {
                    if (command.IsEnabled && this.GetExistingCommand() != command)
                    {
                        if (commandStrings.Any(c => command.Commands.Contains(c)))
                        {
                            await MessageBoxHelper.ShowMessageDialog("There already exists a chat command that uses one of the command strings you have specified");
                            return;
                        }
                    }
                }

                ActionBase chatAction = this.chatActionControl.GetAction();
                if (chatAction == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("The chat message can not be empty");
                    return;
                }

                BasicChatCommand newCommand = new BasicChatCommand(commandStrings, EnumHelper.GetEnumValueFromString<UserRole>((string)this.LowestRoleAllowedComboBox.SelectedItem), cooldown);
                newCommand.Actions.Add(chatAction);

                if (this.command != null)
                {
                    ChannelSession.Settings.ChatCommands.Remove(this.command);
                }
                ChannelSession.Settings.ChatCommands.Add(newCommand);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
