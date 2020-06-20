using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotTimerModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Response { get; set; }
        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        public StreamlabsChatBotTimerModel()
        {
            this.Actions = new List<ActionBase>();
        }

        public StreamlabsChatBotTimerModel(List<string> values)
            : this()
        {
            this.Name = values[0];
            this.Response = values[1];
            this.Enabled = bool.Parse(values[2]);
        }
    }
}
