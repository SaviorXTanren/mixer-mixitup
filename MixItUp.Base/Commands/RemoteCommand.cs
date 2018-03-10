using MixItUp.Base.Model.Remote;
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

        public Guid CommandID { get; set; }

        public RemoteCommand() { }

        public RemoteCommand(string name)
            : base(name, CommandTypeEnum.Remote, name)
        { }

        public RemoteCommand(string name, CommandBase commandToRun)
            : base(name, CommandTypeEnum.Remote, name)
        {
            this.CommandID = commandToRun.ID;
        }

        public CommandBase ReferenceCommand
        {
            get
            {
                if (this.CommandID != Guid.Empty)
                {
                    return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                }
                return null;
            }
        }

        public override bool IsEditable { get { return (this.ReferenceCommand == null); } }

        public RemoteBoardItemModelBase GetRemoteBoardItem()
        {
            return new RemoteBoardButtonModel()
            {
                ID = this.ID,
                Name = this.Name
            };
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return RemoteCommand.remoteCommandPerformSemaphore; } }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, CancellationToken token)
        {
            CommandBase command = this.ReferenceCommand;
            if (command != null)
            {
                await command.Perform(user, arguments);
            }
            else
            {
                await base.PerformInternal(user, arguments, token);
            }
        }
    }
}
