using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.ScorpBot
{
    [DataContract]
    public class ScorpBotData
    {
        [DataMember]
        public Dictionary<string, Dictionary<string, string>> Settings { get; set; }

        [DataMember]
        public UserItemAcquisitonViewModel CurrencyAcquisition { get; set; }

        [DataMember]
        public UserItemAcquisitonViewModel RankAcquisition { get; set; }

        [DataMember]
        public List<ScorpBotViewer> Viewers { get; set; }

        [DataMember]
        public List<ScorpBotCommand> Commands { get; set; }

        [DataMember]
        public List<string> BannedWords { get; set; }

        [DataMember]
        public List<string> Quotes { get; set; }

        [DataMember]
        public List<ScorpBotRank> Ranks { get; set; }

        public ScorpBotData()
        {
            this.Settings = new Dictionary<string, Dictionary<string, string>>();

            this.CurrencyAcquisition = new UserItemAcquisitonViewModel();
            this.RankAcquisition = new UserItemAcquisitonViewModel();

            this.Viewers = new List<ScorpBotViewer>();
            this.Commands = new List<ScorpBotCommand>();
            this.BannedWords = new List<string>();
            this.Quotes = new List<string>();
            this.Ranks = new List<ScorpBotRank>();
        }
    }
}
