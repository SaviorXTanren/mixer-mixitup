using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.ScorpBot
{
    [DataContract]
    public class ScorpBotRank
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public ScorpBotRank() { }

        public ScorpBotRank(DbDataReader reader)
        {
            this.Name = (string)reader["Name"];
            this.Amount = int.Parse((string)reader["Points_v3"]);
        }
    }
}
