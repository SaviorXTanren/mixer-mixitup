using MaterialDesignThemes.Wpf;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for CommandSelectorDialogControl.xaml
    /// </summary>
    public partial class CommandSelectorDialogControl : UserControl
    {
        public CommandBase Command { get; private set; }

        public CommandSelectorDialogControl()
        {
            this.Loaded += CommandSelectorDialogControl_Loaded;

            InitializeComponent();
        }

        private void CommandSelectorDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var types = ChannelSession.AllCommands.Select(c => c.Type).Distinct();
            this.TypeComboBox.ItemsSource = types.OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s));
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                CommandTypeEnum type = (CommandTypeEnum)this.TypeComboBox.SelectedItem;
                IEnumerable<CommandBase> commands = ChannelSession.AllCommands.Where(c => c.Type == type && !(c is PreMadeChatCommand)).OrderBy(c => c.Name);
                this.CommandComboBox.ItemsSource = commands;
            }
        }

        private void CommandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandComboBox.SelectedIndex >= 0)
            {
                this.Command = (CommandBase)this.CommandComboBox.SelectedItem;
                DialogHost.CloseDialogCommand.Execute(this, this);
            }
        }
    }
}
