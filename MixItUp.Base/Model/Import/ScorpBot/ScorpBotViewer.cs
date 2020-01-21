using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.ScorpBot
{
    [DataContract]
    public class ScorpBotViewer
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string Rank { get; set; }

        [DataMember]
        public long RankPoints { get; set; }
        [DataMember]
        public long Currency { get; set; }

        [DataMember]
        public double Hours { get; set; }

        [DataMember]
        public string Sub { get; set; }

        public ScorpBotViewer() { }

        public ScorpBotViewer(Dictionary<string, object> data)
        {
            this.ID = uint.Parse((string)data["BeamID"]);
            this.UserName = (string)data["BeamName"];
            this.Type = (int)data["Type"];
            this.Rank = (data["Rank"] != null && data["Rank"] != DBNull.Value) ? (string)data["Rank"] : string.Empty;
            this.RankPoints = (long)data["Points"];
            this.Currency = (long)data["Points2"];
            this.Hours = double.Parse(data["Hours"].ToString());
            this.Sub = (string)data["Sub"];
        }
    }
}
