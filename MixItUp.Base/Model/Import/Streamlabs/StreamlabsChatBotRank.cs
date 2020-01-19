using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotRank
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Requirement { get; set; }
        [DataMember]
        public string UserGroup { get; set; }
        [DataMember]
        public string Info { get; set; }

        public StreamlabsChatBotRank() { }

        public StreamlabsChatBotRank(List<string> values)
        {
            this.Name = values[0];

            int.TryParse(values[1], out int requirement);
            this.Requirement = requirement;

            this.UserGroup = values[2];

            this.Info = values[3];
        }
    }
}
