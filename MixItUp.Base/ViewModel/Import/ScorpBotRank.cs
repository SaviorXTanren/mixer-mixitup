using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
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

            int amount = 0;
            int.TryParse((string)reader["Points_v3"], out amount);
            this.Amount = amount;
        }
    }
}
