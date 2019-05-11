using Mixer.Base.Model.User;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.AutoHoster
{
    [DataContract]
    public class ChannelHostModel
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public bool IsOnline { get; set; }

        public ChannelHostModel() { }

        public ChannelHostModel(UserModel user)
        {
            this.ID = user.id;
            this.Name = user.username;
            this.IsEnabled = true;
        }
    }
}
