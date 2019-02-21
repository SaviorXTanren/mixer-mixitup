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

        public RemoteCommandItemModel(int xPosition, int yPosition)
            : base(xPosition, yPosition)
        { }
    }
}
