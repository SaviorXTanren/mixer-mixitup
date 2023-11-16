using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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
        public ObservableCollection<NewAutoChatCommandModel> commands = new ObservableCollection<NewAutoChatCommandModel>();

        public NewAutoChatCommandsDialogControl(IEnumerable<NewAutoChatCommandModel> commands)
        {
            InitializeComponent();

            this.NewCommandsItemsControl.ItemsSource = this.commands;
            this.commands.AddRange(commands);
        }

        public void AddSelectedCommands()
        {
            foreach (NewAutoChatCommandModel command in this.commands)
            {
                if (command.AddCommand)
                {
                    ChannelSession.Settings.SetCommand(command.Command);
                    ServiceManager.Get<CommandService>().ChatCommands.Add(command.Command);
                }
            }
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
        }
    }
}
