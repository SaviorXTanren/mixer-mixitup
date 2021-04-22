using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class ActionGroupCommandModel : CommandModelBase
    {
        [DataMember]
        public bool RunOneRandomly { get; set; }

        public ActionGroupCommandModel(string name, bool runOneRandomly)
            : base(name, CommandTypeEnum.ActionGroup)
        {
            this.RunOneRandomly = runOneRandomly;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal ActionGroupCommandModel(MixItUp.Base.Commands.ActionGroupCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.ActionGroup;
            this.RunOneRandomly = command.IsRandomized;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        protected ActionGroupCommandModel() : base() { }
    }
}
