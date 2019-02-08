using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models.Items
{
    [DataContract]
    public class RemoteFolderItemModel : RemoteButtonItemModelBase
    {
        [DataMember]
        public RemoteBoardModel Board { get; set; }

        public RemoteFolderItemModel() { }

        public RemoteFolderItemModel(int xPosition, int yPosition)
            : base(xPosition, yPosition)
        {
            this.Board = new RemoteBoardModel(isSubBoard: true);
        }
    }
}
