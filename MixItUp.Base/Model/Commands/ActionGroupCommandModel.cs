using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class ActionGroupCommandModel : CommandModelBase
    {
        [Obsolete]
        [DataMember]
        public bool RunOneRandomly { get; set; }

        public ActionGroupCommandModel(string name)
            : base(name, CommandTypeEnum.ActionGroup)
        { }

        [Obsolete]
        public ActionGroupCommandModel() : base() { }
    }
}
