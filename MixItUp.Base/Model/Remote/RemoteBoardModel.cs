using MixItUp.Base.Remote.Models.Items;
using System;
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
        public Guid ID { get; set; }

        [DataMember]
        public bool IsSubBoard { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string ImagePath { get; set; }

        [DataMember]
        public List<RemoteItemModelBase> Items { get; set; }

        public RemoteBoardModel()
        {
            this.ID = Guid.NewGuid();
            this.Items = new List<RemoteItemModelBase>();
        }

        public RemoteBoardModel(bool isSubBoard)
            : this()
        {
            this.IsSubBoard = isSubBoard;
        }

        public RemoteItemModelBase GetItem(int xPosition, int yPosition) { return this.Items.FirstOrDefault(i => i.XPosition == xPosition && i.YPosition == yPosition); }

        public void SetItem(RemoteItemModelBase item, int xPosition, int yPosition)
        {
            RemoteItemModelBase existingItem = this.GetItem(xPosition, yPosition);
            if (existingItem != null)
            {
                this.Items.Remove(existingItem);
            }

            if (item != null)
            {
                item.XPosition = xPosition;
                item.YPosition = yPosition;
                this.Items.Add(item);
            }
        }
    }
}
