using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import
{
    [DataContract]
    public class StreamlabsChatBotTimer
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Response { get; set; }
        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        public StreamlabsChatBotTimer()
        {
            this.Actions = new List<ActionBase>();
        }

        public StreamlabsChatBotTimer(List<string> values)
            : this()
        {
            this.Name = values[0];
            this.Response = values[1];
            this.Enabled = bool.Parse(values[2]);
        }
    }
}
