using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public partial class CommandActionControl : ActionControlBase
    {
        private const string PreMadeCommandType = "Pre-Made";

        private CommandAction command;

        public CommandActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public CommandActionControl(ActionContainerControl containerControl, CommandAction action) : this(containerControl) { this.command = action; }

        public override Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CommandActionTypeEnum>().OrderBy(s => s);

            List<string> types = new List<string>(EnumHelper.GetEnumNames(ChannelSession.AllCommands.Select(c => c.Type).Distinct()));
            types.Add(PreMadeCommandType);
            this.CommandTypeComboBox.ItemsSource = types.OrderBy(s => s);

            this.CommandGroupNameComboBox.ItemsSource = ChannelSession.Settings.CommandGroups.Keys;

            this.CommandNameComboBox.ItemsSource = ChannelSession.AllCommands.OrderBy(c => c.Name);
            if (this.command != null)
            {
                CommandBase chosenCommand = this.command.Command;
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.CommandActionType);
                if (chosenCommand != null)
                {
                    string type = EnumHelper.GetEnumName(chosenCommand.Type);
                    if (chosenCommand is PreMadeChatCommand)
                    {
                        type = PreMadeCommandType;
                    }
                    this.CommandTypeComboBox.SelectedItem = type;
                    this.CommandNameComboBox.SelectedItem = chosenCommand;
                }
                this.CommandGroupNameComboBox.SelectedItem = this.command.GroupName;
                this.CommandArgumentsTextBox.Text = this.command.CommandArguments;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            CommandActionTypeEnum type = EnumHelper.GetEnumValueFromString<CommandActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
            if (type == CommandActionTypeEnum.DisableCommandGroup || type == CommandActionTypeEnum.EnableCommandGroup)
            {
                if (this.CommandGroupNameComboBox.SelectedIndex >= 0)
                {
                    return new CommandAction(type, this.CommandGroupNameComboBox.Text);
                }
            }
            else
            {
                if (this.CommandNameComboBox.SelectedIndex >= 0)
                {
                    CommandBase command = (CommandBase)this.CommandNameComboBox.SelectedItem;
                    return new CommandAction(type, command, this.CommandArgumentsTextBox.Text);
                }
            }
            return null;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CommandGrid.Visibility = Visibility.Collapsed;
            this.CommandGroupGrid.Visibility = Visibility.Collapsed;
            this.CommandArgumentsTextBox.Visibility = Visibility.Collapsed;

            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                CommandActionTypeEnum type = EnumHelper.GetEnumValueFromString<CommandActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                switch (type)
                {
                    case CommandActionTypeEnum.RunCommand:
                        this.CommandGrid.Visibility = Visibility.Visible;
                        this.CommandArgumentsTextBox.Visibility = Visibility.Visible;
                        break;
                    case CommandActionTypeEnum.DisableCommand:
                    case CommandActionTypeEnum.EnableCommand:
                        this.CommandGrid.Visibility = Visibility.Visible;
                        break;
                    case CommandActionTypeEnum.DisableCommandGroup:
                    case CommandActionTypeEnum.EnableCommandGroup:
                        this.CommandGroupGrid.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void CommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandTypeComboBox.SelectedIndex >= 0)
            {
                string typeString = (string)this.CommandTypeComboBox.SelectedItem;
                CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeString);
                if (typeString.Equals(PreMadeCommandType))
                {
                    type = CommandTypeEnum.Chat;
                }

                IEnumerable<CommandBase> commands = ChannelSession.AllCommands.Where(c => c.Type == type).OrderBy(c => c.Name);
                if (type == CommandTypeEnum.Chat)
                {
                    if (typeString.Equals(PreMadeCommandType))
                    {
                        commands = commands.Where(c => c is PreMadeChatCommand);
                    }
                    else
                    {
                        commands = commands.Where(c => !(c is PreMadeChatCommand));
                    }
                }
                this.CommandNameComboBox.ItemsSource = commands;
            }
        }
    }
}
