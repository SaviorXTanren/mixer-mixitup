using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for RemoteReferenceCommandDialogControl.xaml
    /// </summary>
    public partial class RemoteReferenceCommandDialogControl : UserControl
    {
        public CommandBase ReferenceCommand { get; private set; }

        public RemoteReferenceCommandDialogControl()
        {
            InitializeComponent();

            this.Loaded += RemoteReferenceCommandDialogControl_Loaded;
        }

        private void RemoteReferenceCommandDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
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
                IEnumerable<CommandBase> commands = ChannelSession.AllEnabledCommands.Where(c => c.Type == commandType);
                if (commandType == CommandTypeEnum.Chat)
                {
                    commands = commands.Where(c => !(c is PreMadeChatCommand));
                }
                this.CommandNameComboBox.ItemsSource = commands.OrderBy(c => c.Name);
                this.CommandNameComboBox.SelectedIndex = -1;
                this.CommandNameComboBox.IsEnabled = true;
            }
            this.SaveButton.IsEnabled = false;
        }

        private void CommandNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandNameComboBox.SelectedIndex >= 0)
            {
                this.ReferenceCommand = (CommandBase)this.CommandNameComboBox.SelectedItem;
                this.SaveButton.IsEnabled = true;
            }
        }
    }
}
