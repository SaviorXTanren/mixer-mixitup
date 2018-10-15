using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import
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

        public ScorpBotViewer(DbDataReader reader)
        {
            this.ID = uint.Parse((string)reader["BeamID"]);
            this.UserName = (string)reader["BeamName"];
            this.Type = (int)reader["Type"];
            this.Rank = (reader["Rank"] != null && reader["Rank"] != DBNull.Value) ? (string)reader["Rank"] : string.Empty;
            this.RankPoints = (long)reader["Points"];
            this.Currency = (long)reader["Points2"];
            this.Hours = double.Parse(reader["Hours"].ToString());
            this.Sub = (string)reader["Sub"];
        }
    }
}
