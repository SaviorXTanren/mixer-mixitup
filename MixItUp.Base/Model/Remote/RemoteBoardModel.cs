using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote
{
    public enum RemoteBoardItemSizeEnum
    {
        OneByOne,
        TwoByOne,
        TwoByTwo,
    }

    [DataContract]
    public class RemoteBoardModel
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BackgroundImageName { get; set; }

        [DataMember]
        public List<RemoteBoardGroupModel> Groups { get; set; }
        [DataMember]
        public Dictionary<string, string> Images { get; set; }

        public RemoteBoardModel()
        {
            this.Groups = new List<RemoteBoardGroupModel>();
            this.Images = new Dictionary<string, string>();
        }

        public RemoteBoardModel(string name)
            : this()
        {
            this.ID = Guid.NewGuid();
            this.Name = name;

            this.Groups.Add(new RemoteBoardGroupModel("Default"));
        }

        public RemoteBoardModel ToSimpleModel()
        {
            return new RemoteBoardModel() { ID = this.ID, Name = this.Name };
        }
    }

    [DataContract]
    public class RemoteBoardGroupModel
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<RemoteBoardItemModelBase> Items { get; set; }

        public RemoteBoardGroupModel()
        {
            this.Items = new List<RemoteBoardItemModelBase>();
        }

        public RemoteBoardGroupModel(string name)
            : this()
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }
    }
}
