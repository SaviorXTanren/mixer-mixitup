using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Remote.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteCommandItemViewModel : RemoteButtonItemViewModelBase
    {
        private new RemoteCommandItemModel model;

        public RemoteCommandItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteCommandItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteCommandItemViewModel(RemoteCommandItemModel model)
            : base(model)
        {
            this.model = model;

            CommandBase command = this.Command;
            if (command != null)
            {
                this.CommandType = EnumHelper.GetEnumName(command.Type);
            }
        }

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

        public Guid CommandID { get { return this.model.CommandID; } set { this.model.CommandID = value; } }

        public CommandBase Command
        {
            get { return ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); }
            set
            {
                if (value != null)
                {
                    this.CommandID = value.ID;
                }
                else
                {
                    this.CommandID = Guid.Empty;
                }
                this.NotifyPropertyChanged();
            }
        }

        public override bool IsCommand { get { return true; } }
    }
}
