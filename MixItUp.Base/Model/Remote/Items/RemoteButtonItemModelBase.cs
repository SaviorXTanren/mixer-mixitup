using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models.Items
{
    [DataContract]
    public abstract class RemoteButtonItemModelBase : RemoteItemModelBase
    {
        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string ImageData { get; set; }

        public RemoteButtonItemModelBase() { }

        public RemoteButtonItemModelBase(int xPosition, int yPosition) : base(xPosition, yPosition) { }
    }
}
