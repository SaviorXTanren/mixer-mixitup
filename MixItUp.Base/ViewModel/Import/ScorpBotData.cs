using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Numerics;
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
        public List<string> FilteredWords { get; set; }

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
            this.FilteredWords = new List<string>();
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

        public int GetIntSettingsValue(string key, string value)
        {
            BigInteger bigInt = BigInteger.Parse(this.GetSettingsValue(key, value, "0"));
            if (bigInt > int.MaxValue)
            {
                return int.MaxValue;
            }

            if (bigInt < int.MinValue)
            {
                return int.MinValue;
            }
            return (int)bigInt;
        }

        public bool GetBoolSettingsValue(string key, string value)
        {
            return (this.GetIntSettingsValue(key, value) == 1);
        }

        public MixerRoleEnum GetUserRoleSettingsValue(string key, string value)
        {
            switch (this.GetIntSettingsValue(key, value))
            {
                case 0: return MixerRoleEnum.User;
                case 1: return MixerRoleEnum.Subscriber;
                case 2: return MixerRoleEnum.Mod;
                case 3:
                case 4:
                default:
                    return MixerRoleEnum.Streamer;
            }
        }
    }
}
