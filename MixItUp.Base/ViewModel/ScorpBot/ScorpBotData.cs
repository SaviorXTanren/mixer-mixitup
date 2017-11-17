using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.ScorpBot
{
    [DataContract]
    public class ScorpBotData
    {
        [DataMember]
        public List<ScorpBotViewer> Viewers { get; set; }

        public ScorpBotData()
        {
            this.Viewers = new List<ScorpBotViewer>();
        }
    }
}
