using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public class RemoteCommandItemControlViewModel : RemoteItemControlViewModelBase
    {
        public const string RemoteCommandDetailsEventName = "RemoteCommandDetails";

        public RemoteCommandItemControlViewModel(RemoteCommandItemViewModel item)
            : base(item)
        {
            CommandBase command = this.Command;
            if (command != null)
            {
                this.CommandType = EnumHelper.GetEnumName(command.Type);
            }

            this.CommandSelectedCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteCommandItemControlViewModel>(RemoteCommandItemControlViewModel.RemoteCommandDetailsEventName, this);
                return Task.FromResult(0);
            });
        }

        public ICommand CommandSelectedCommand { get; private set; }

        public IEnumerable<string> CommandTypes { get { return EnumHelper.GetEnumNames(ChannelSession.AllEnabledCommands.Select(c => c.Type).Distinct()); } }
        public string CommandType
        {
            get { return this.commandType; }
            set
            {
                this.commandType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("Commands");
                this.NotifyPropertyChanged("Command");
            }
        }
        private string commandType;

        public IEnumerable<CommandBase> Commands
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CommandType))
                {
                    CommandTypeEnum commandType = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(this.CommandType);
                    return ChannelSession.AllEnabledCommands.Where(c => c.Type == commandType && !(c is PreMadeChatCommand)).OrderBy(c => c.Name);
                }
                return null;
            }
        }

        public CommandBase Command
        {
            get { return ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.GetTypedItem<RemoteCommandItemViewModel>().CommandID)); }
            set
            {
                if (value != null)
                {
                    this.GetTypedItem<RemoteCommandItemViewModel>().CommandID = value.ID;
                }
                else
                {
                    this.GetTypedItem<RemoteCommandItemViewModel>().CommandID = Guid.Empty;
                }
                this.NotifyPropertyChanged();
            }
        }
    }
}
