using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models.Items
{
    public enum RemoteItemSizeEnum
    {
        OneByOne,
        OneByTwo,
        TwoByOne,
        TwoByTwo,
    }

    [DataContract]
    public abstract class RemoteItemModelBase
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public RemoteItemSizeEnum Size { get; set; }

        [DataMember]
        public int XPosition { get; set; }
        [DataMember]
        public int YPosition { get; set; }

        public RemoteItemModelBase() { }

        public RemoteItemModelBase(int xPosition, int yPosition)
        {
            this.ID = Guid.NewGuid();
            this.Name = string.Empty;
            this.Size = RemoteItemSizeEnum.OneByOne;
            this.XPosition = xPosition;
            this.YPosition = yPosition;
        }
    }
}
