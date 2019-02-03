using MixItUp.Base.Remote.Models.Items;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models
{
    [DataContract]
    public class RemoteBoardModel
    {
        public const int BoardWidth = 5;
        public const int BoardHeight = 3;

        [DataMember]
        public bool IsSubBoard { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string ImageData { get; set; }

        [DataMember]
        public List<RemoteItemModelBase> Items { get; set; }

        public RemoteBoardModel()
        {
            this.Items = new List<RemoteItemModelBase>();
        }

        public RemoteBoardModel(bool isSubBoard)
            : this()
        {
            this.IsSubBoard = isSubBoard;
        }

        public RemoteItemModelBase GetItem(int xPosition, int yPosition) { return this.Items.FirstOrDefault(i => i.XPosition == xPosition && i.YPosition == yPosition); }
    }
}
