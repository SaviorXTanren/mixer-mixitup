using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotViewer
    {
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; }
        [DataMember]
        public uint ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Rank { get; set; }
        [DataMember]
        public int Points { get; set; }
        [DataMember]
        public int Hours { get; set; }

        public StreamlabsChatBotViewer() { }

        public StreamlabsChatBotViewer(StreamingPlatformTypeEnum platform, List<string> values)
        {
            this.Platform = platform;

            this.Name = values[0];
            this.Rank = values[1];

            int.TryParse(values[2], out int points);
            this.Points = points;

            int.TryParse(values[3], out int hours);
            this.Hours = hours;
        }
    }
}
