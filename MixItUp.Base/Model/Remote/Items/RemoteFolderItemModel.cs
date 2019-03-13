using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models.Items
{
    [DataContract]
    public class RemoteFolderItemModel : RemoteButtonItemModelBase
    {
        [DataMember]
        public Guid BoardID { get; set; }

        public RemoteFolderItemModel() { }

        public RemoteFolderItemModel(int xPosition, int yPosition) : base(xPosition, yPosition) { }
    }
}
