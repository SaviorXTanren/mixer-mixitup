using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Import.ScorpBot
{
    [DataContract]
    public class ScorpBotData
    {
        public static async Task<ScorpBotData> GatherScorpBotData(string folderPath)
        {
            try
            {
                ScorpBotData scorpBotData = new ScorpBotData();

                string dataPath = Path.Combine(folderPath, "Data");
                string settingsFilePath = Path.Combine(dataPath, "settings.ini");
                if (Directory.Exists(dataPath) && File.Exists(settingsFilePath))
                {
                    IEnumerable<string> lines = File.ReadAllLines(settingsFilePath);

                    string currentGroup = null;
                    foreach (var line in lines)
                    {
                        if (line.Contains("="))
                        {
                            string[] splits = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                scorpBotData.Settings[currentGroup][splits[0]] = splits[1];
                            }
                        }
                        else
                        {
                            currentGroup = line.Replace("[", "").Replace("]", "").ToLower();
                            scorpBotData.Settings[currentGroup] = new Dictionary<string, string>();
                        }
                    }

                    string databasePath = Path.Combine(dataPath, "Database");
                    if (Directory.Exists(databasePath))
                    {
                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "CommandsDB.sqlite"), "SELECT * FROM RegCommand",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.Commands.Add(new ScorpBotCommand(data));
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "Timers2DB.sqlite"), "SELECT * FROM TimeCommand",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.Timers.Add(new ScorpBotTimer(data));
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "FilteredWordsDB.sqlite"), "SELECT * FROM Word",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.FilteredWords.Add(((string)data["word"]).ToLower());
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "QuotesDB.sqlite"), "SELECT * FROM Quotes",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.Quotes.Add((string)data["quote_text"]);
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "RankDB.sqlite"), "SELECT * FROM Rank",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.Ranks.Add(new ScorpBotRank(data));
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "Viewers3DB.sqlite"), "SELECT * FROM Viewer",
                        (Dictionary<string, object> data) =>
                        {
                            if (data["BeamID"] != null && int.TryParse((string)data["BeamID"], out int id))
                            {
                                scorpBotData.Viewers.Add(new ScorpBotViewer(data));
                            }
                        });
                    }
                }

                return scorpBotData;
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

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
            try
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
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return 0;
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
