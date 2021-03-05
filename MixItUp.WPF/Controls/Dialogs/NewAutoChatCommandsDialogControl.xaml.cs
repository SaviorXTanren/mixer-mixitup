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
        public ObservableCollection<NewAutoChatCommandModel> commands = new ObservableCollection<NewAutoChatCommandModel>().EnableSync();

        public NewAutoChatCommandsDialogControl(IEnumerable<NewAutoChatCommandModel> commands)
        {
            InitializeComponent();

            this.NewCommandsItemsControl.ItemsSource = this.commands;
            foreach (NewAutoChatCommandModel command in commands)
            {
                this.commands.Add(command);
            }
        }

        public void AddSelectedCommands()
        {
            foreach (NewAutoChatCommandModel command in this.commands)
            {
                if (command.AddCommand)
                {
                    ChannelSession.Settings.SetCommand(command.Command);
                    ChannelSession.ChatCommands.Add(command.Command);
                }
            }
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
        }
    }
}
