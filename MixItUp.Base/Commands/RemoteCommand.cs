using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class RemoteCommand : CommandBase
    {
        private static SemaphoreSlim remoteCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string ImageName { get; set; }

        public RemoteCommand() { }

        public RemoteCommand(string name)
            : base(name, CommandTypeEnum.Remote, name)
        {
            this.BackgroundColor = "#FFFFFF";
            this.TextColor = "#000000";
        }

        public RemoteCommand(string name, CommandBase commandToRun)
            : this(name)
        {
            this.CommandID = commandToRun.ID;
        }

        public CommandBase ReferenceCommand
        {
            get
            {
                if (this.CommandID != Guid.Empty)
                {
                    return ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                }
                return null;
            }
        }

        public override bool IsEditable { get { return (this.ReferenceCommand == null); } }

        protected override SemaphoreSlim AsyncSemaphore { get { return RemoteCommand.remoteCommandPerformSemaphore; } }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            CommandBase command = this.ReferenceCommand;
            if (command != null)
            {
                await command.Perform(user, arguments, extraSpecialIdentifiers);
            }
            else
            {
                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }
    }
}
