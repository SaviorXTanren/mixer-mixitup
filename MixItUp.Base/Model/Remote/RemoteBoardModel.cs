using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Remote
{
    public class RemoteBoardModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<RemoteBoardGroupModel> Groups { get; set; }

        public RemoteBoardModel()
        {
            this.Groups = new List<RemoteBoardGroupModel>();
        }
    }

    public class RemoteBoardGroupModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<RemoteBoardPositionItemModel> Items { get; set; }

        public RemoteBoardGroupModel()
        {
            this.Items = new List<RemoteBoardPositionItemModel>();
        }
    }

    public class RemoteBoardPositionItemModel
    {
        public Guid ID { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Guid ItemID { get; set; }

        public RemoteBoardItemModelBase Item { get; set; }
    }
}
