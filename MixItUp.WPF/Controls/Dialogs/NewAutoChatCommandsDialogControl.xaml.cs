using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for NewAutoChatCommandsDialogControl.xaml
    /// </summary>
    public partial class NewAutoChatCommandsDialogControl : UserControl
    {
        public ObservableCollection<NewAutoChatCommand> commands = new ObservableCollection<NewAutoChatCommand>();

        public NewAutoChatCommandsDialogControl(IEnumerable<NewAutoChatCommand> commands)
        {
            InitializeComponent();

            this.NewCommandsItemsControl.ItemsSource = this.commands;
            foreach (NewAutoChatCommand command in commands)
            {
                this.commands.Add(command);
            }
        }

        public void AddSelectedCommands()
        {
            foreach (NewAutoChatCommand command in this.commands)
            {
                if (command.AddCommand)
                {
                    ChannelSession.Settings.ChatCommands.Add(command.Command);
                }
            }
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }
    }
}
