using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class ScorpBotData
    {
        [DataMember]
        public Dictionary<string, Dictionary<string, string>> Settings { get; set; }

        [DataMember]
        public List<ScorpBotViewer> Viewers { get; set; }

        [DataMember]
        public List<ScorpBotCommand> Commands { get; set; }

        [DataMember]
        public List<ScorpBotTimer> Timers { get; set; }

        [DataMember]
        public List<string> BannedWords { get; set; }

        [DataMember]
        public List<string> Quotes { get; set; }

        [DataMember]
        public List<ScorpBotRank> Ranks { get; set; }

        public ScorpBotData()
        {
            this.Settings = new Dictionary<string, Dictionary<string, string>>();

            this.Viewers = new List<ScorpBotViewer>();
            this.Commands = new List<ScorpBotCommand>();
            this.Timers = new List<ScorpBotTimer>();
            this.BannedWords = new List<string>();
            this.Quotes = new List<string>();
            this.Ranks = new List<ScorpBotRank>();
        }

        public string GetSettingsValue(string key, string value, string defaultValue)
        {
            if (this.Settings.ContainsKey(key) && this.Settings[key].ContainsKey(value))
            {
                return this.Settings[key][value];
            }
            return defaultValue;
        }
    }
}
