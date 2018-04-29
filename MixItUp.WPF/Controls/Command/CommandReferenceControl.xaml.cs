using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for CommandReferenceControl.xaml
    /// </summary>
    public partial class CommandReferenceControl : UserControl
    {
        private CommandBase command;
        public CommandBase Command
        {
            get { return this.command; }
            set
            {
                this.command = value;
                if (this.Command != null)
                {
                    this.CommandTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.Command.Type);
                    this.CommandNameComboBox.SelectedItem = this.Command;
                }
            }
        }

        public CommandReferenceControl()
        {
            InitializeComponent();

            List<CommandTypeEnum> commandTypes = EnumHelper.GetEnumList<CommandTypeEnum>().ToList();
            commandTypes.Remove(CommandTypeEnum.Game);
            commandTypes.Remove(CommandTypeEnum.Remote);
            commandTypes.Remove(CommandTypeEnum.Custom);
            this.CommandTypeComboBox.ItemsSource = EnumHelper.GetEnumNames(commandTypes);
        }

        private void CommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandTypeComboBox.SelectedIndex >= 0)
            {
                CommandTypeEnum commandType = EnumHelper.GetEnumValueFromString<CommandTypeEnum>((string)this.CommandTypeComboBox.SelectedItem);
                IEnumerable<CommandBase> commands = ChannelSession.AllCommands.Where(c => c.Type == commandType);
                if (commandType == CommandTypeEnum.Chat)
                {
                    commands = commands.Where(c => !(c is PreMadeChatCommand));
                }
                this.CommandNameComboBox.ItemsSource = commands.OrderBy(c => c.Name);
                this.CommandNameComboBox.SelectedIndex = -1;
                this.CommandNameComboBox.IsEnabled = true;
            }
        }

        private void CommandNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandNameComboBox.SelectedIndex >= 0 && this.Command != this.CommandNameComboBox.SelectedItem)
            {
                this.Command = (CommandBase)this.CommandNameComboBox.SelectedItem;
            }
        }
    }
}
