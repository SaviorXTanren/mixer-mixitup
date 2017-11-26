using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.ScorpBot
{
    [DataContract]
    public class ScorpBotData
    {
        [DataMember]
        public List<ScorpBotViewer> Viewers { get; set; }

        [DataMember]
        public List<ScorpBotCommand> Commands { get; set; }

        public ScorpBotData()
        {
            this.Viewers = new List<ScorpBotViewer>();
            this.Commands = new List<ScorpBotCommand>();
        }
    }
}
