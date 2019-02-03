using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models.Items
{
    [DataContract]
    public class RemoteCommandItemModel : RemoteButtonItemModelBase
    {
        [DataMember]
        public Guid CommandID { get; set; }

        public RemoteCommandItemModel() { }

        public RemoteCommandItemModel(Guid commandID, int xPosition, int yPosition)
            : base(xPosition, yPosition)
        {
            this.CommandID = commandID;
        }
    }
}
