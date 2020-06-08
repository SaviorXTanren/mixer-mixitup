using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.ScorpBot
{
    [DataContract]
    public class ScorpBotRankModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public ScorpBotRankModel() { }

        public ScorpBotRankModel(Dictionary<string, object> data)
        {
            this.Name = (string)data["Name"];

            int amount = 0;
            int.TryParse((string)data["Points_v3"], out amount);
            this.Amount = amount;
        }
    }
}
