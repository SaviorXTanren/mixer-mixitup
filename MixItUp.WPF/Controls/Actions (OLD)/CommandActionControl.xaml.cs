using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public partial class CommandActionControl : ActionControlBase
    {
        private readonly string PreMadeCommandType = MixItUp.Base.Resources.PreMade;

        private CommandAction command;

        public CommandActionControl() : base() { InitializeComponent(); }

        public CommandActionControl(CommandAction action) : this() { this.command = action; }

        public override Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = Enum.GetValues(typeof(CommandActionTypeEnum))
                .Cast<CommandActionTypeEnum>()
                .OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s));

            List<string> types = new List<string>(ChannelSession.AllCommands.Select(c => EnumLocalizationHelper.GetLocalizedName(c.Type)).Distinct());
            types.Add(PreMadeCommandType);
            this.CommandTypeComboBox.ItemsSource = types.OrderBy(s => s);

            this.CommandGroupNameComboBox.ItemsSource = ChannelSession.Settings.CommandGroups.Keys;

            this.CommandNameComboBox.ItemsSource = ChannelSession.AllCommands.OrderBy(c => c.Name);
            if (this.command != null)
            {
                CommandBase chosenCommand = this.command.Command;
                this.TypeComboBox.SelectedItem = this.command.CommandActionType;
                if (chosenCommand != null)
                {
                    string type = EnumLocalizationHelper.GetLocalizedName(chosenCommand.Type);
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
            CommandActionTypeEnum type = (CommandActionTypeEnum)this.TypeComboBox.SelectedItem;
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
                CommandActionTypeEnum type = (CommandActionTypeEnum)this.TypeComboBox.SelectedItem;
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
                if (typeString.Equals(PreMadeCommandType))
                {
                    this.CommandNameComboBox.ItemsSource = ChannelSession.AllCommands.Where(c => c is PreMadeChatCommand && c.Type == CommandTypeEnum.Chat).OrderBy(c => c.Name);
                }
                else if (typeString.Equals(EnumLocalizationHelper.GetLocalizedName(CommandTypeEnum.Chat)))
                {
                    this.CommandNameComboBox.ItemsSource = ChannelSession.AllCommands.Where(c => !(c is PreMadeChatCommand) && c.Type == CommandTypeEnum.Chat).OrderBy(c => c.Name);
                }
                else
                {
                    this.CommandNameComboBox.ItemsSource = ChannelSession.AllCommands.Where(c => typeString.Equals(EnumLocalizationHelper.GetLocalizedName(c.Type))).OrderBy(c => c.Name);
                }
            }
        }
    }
}
