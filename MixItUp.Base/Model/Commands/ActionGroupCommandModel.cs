using System;
using System.Runtime.Serialization;

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

        [Obsolete]
        public ActionGroupCommandModel() : base() { }
    }
}
