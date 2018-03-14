using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Remote
{
    public enum RemoteBoardItemSizeEnum
    {
        OneByOne,
        TwoByOne,
        TwoByTwo,
    }

    public class RemoteBoardModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public string BackgroundColor { get; set; }
        public string BackgroundImageName { get; set; }

        public List<RemoteBoardGroupModel> Groups { get; set; }
        public Dictionary<string, string> Images { get; set; }

        public RemoteBoardModel()
        {
            this.Groups = new List<RemoteBoardGroupModel>();
            this.Images = new Dictionary<string, string>();
        }
    }

    public class RemoteBoardGroupModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public List<RemoteBoardItemModelBase> Items { get; set; }

        public RemoteBoardGroupModel()
        {
            this.Items = new List<RemoteBoardItemModelBase>();
        }
    }
}
