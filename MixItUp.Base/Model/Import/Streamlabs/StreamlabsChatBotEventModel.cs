using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotEventModel
    {
        [DataMember]
        public uint UserID { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Mode { get; set; }
        [DataMember]
        public bool ClientOnly { get; set; }
        [DataMember]
        public int Volume { get; set; }
        [DataMember]
        public bool Enabled { get; set; }

        public StreamlabsChatBotEventModel() { }

        public StreamlabsChatBotEventModel(List<string> values)
        {
            uint.TryParse(values[0], out uint userID);
            this.UserID = userID;

            this.Message = values[1];
            this.Mode = values[2];

            this.ClientOnly = bool.Parse(values[3]);

            int.TryParse(values[4], out int volume);
            this.Volume = volume;

            this.Enabled = bool.Parse(values[5]);
        }
    }
}
