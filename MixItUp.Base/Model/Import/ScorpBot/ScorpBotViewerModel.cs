using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.ScorpBot
{
    [DataContract]
    public class ScorpBotViewerModel
    {
        [DataMember]
        public uint MixerID { get; set; }
        [DataMember]
        public string MixerUsername { get; set; }

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

        public ScorpBotViewerModel() { }

        public ScorpBotViewerModel(Dictionary<string, object> data)
        {
            this.MixerID = uint.Parse((string)data["BeamID"]);
            this.MixerUsername = (string)data["BeamName"];
            this.Type = (int)data["Type"];
            this.Rank = (data["Rank"] != null && data["Rank"] != DBNull.Value) ? (string)data["Rank"] : string.Empty;
            this.RankPoints = (long)data["Points"];
            this.Currency = (long)data["Points2"];
            this.Hours = double.Parse(data["Hours"].ToString());
            this.Sub = (string)data["Sub"];
        }
    }
}
