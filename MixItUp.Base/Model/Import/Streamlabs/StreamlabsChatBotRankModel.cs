using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotRankModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Requirement { get; set; }
        [DataMember]
        public string UserGroup { get; set; }
        [DataMember]
        public string Info { get; set; }

        public StreamlabsChatBotRankModel() { }

        public StreamlabsChatBotRankModel(List<string> values)
        {
            this.Name = values[0];

            int.TryParse(values[1], out int requirement);
            this.Requirement = requirement;

            this.UserGroup = values[2];

            this.Info = values[3];
        }
    }
}
