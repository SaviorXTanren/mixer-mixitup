using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
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
    public class ScorpBotDataModel
    {
        public static async Task<ScorpBotDataModel> GatherScorpBotData(string folderPath)
        {
            try
            {
                ScorpBotDataModel scorpBotData = null;
                string dataPath = Path.Combine(folderPath, "Data");
                string settingsFilePath = Path.Combine(dataPath, "settings.ini");
                if (Directory.Exists(dataPath) && File.Exists(settingsFilePath))
                {
                    scorpBotData = new ScorpBotDataModel();

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
                            scorpBotData.Commands.Add(new ScorpBotCommandModel(data));
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "Timers2DB.sqlite"), "SELECT * FROM TimeCommand",
                        (Dictionary<string, object> data) =>
                        {
                            scorpBotData.Timers.Add(new ScorpBotTimerModel(data));
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
                            scorpBotData.Ranks.Add(new ScorpBotRankModel(data));
                        });

                        await ChannelSession.Services.Database.Read(Path.Combine(databasePath, "Viewers3DB.sqlite"), "SELECT * FROM Viewer",
                        (Dictionary<string, object> data) =>
                        {
                            if (data["BeamID"] != null && int.TryParse((string)data["BeamID"], out int id))
                            {
                                scorpBotData.Viewers.Add(new ScorpBotViewerModel(data));
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
        public List<ScorpBotViewerModel> Viewers { get; set; }

        [DataMember]
        public List<ScorpBotCommandModel> Commands { get; set; }

        [DataMember]
        public List<ScorpBotTimerModel> Timers { get; set; }

        [DataMember]
        public List<string> FilteredWords { get; set; }

        [DataMember]
        public List<string> Quotes { get; set; }

        [DataMember]
        public List<ScorpBotRankModel> Ranks { get; set; }

        public ScorpBotDataModel()
        {
            this.Settings = new Dictionary<string, Dictionary<string, string>>();

            this.Viewers = new List<ScorpBotViewerModel>();
            this.Commands = new List<ScorpBotCommandModel>();
            this.Timers = new List<ScorpBotTimerModel>();
            this.FilteredWords = new List<string>();
            this.Quotes = new List<string>();
            this.Ranks = new List<ScorpBotRankModel>();
        }

        public void ImportSettings()
        {
            // Import Ranks
            int rankEnabled = this.GetIntSettingsValue("currency", "enabled");
            string rankName = this.GetSettingsValue("currency", "name", "Rank");
            int rankInterval = this.GetIntSettingsValue("currency", "onlinepayinterval");
            int rankAmount = this.GetIntSettingsValue("currency", "activeuserbonus");
            int rankMaxAmount = this.GetIntSettingsValue("currency", "maxlimit");
            if (rankMaxAmount <= 0)
            {
                rankMaxAmount = int.MaxValue;
            }
            int rankOnFollowBonus = this.GetIntSettingsValue("currency", "onfollowbonus");
            int rankOnSubBonus = this.GetIntSettingsValue("currency", "onsubbonus");
            int rankSubBonus = this.GetIntSettingsValue("currency", "subbonus");
            string rankCommand = this.GetSettingsValue("currency", "command", "");
            string rankCommandResponse = this.GetSettingsValue("currency", "response", "");
            string rankUpCommand = this.GetSettingsValue("currency", "Currency1RankUpMsg", "");
            int rankAccumulationType = this.GetIntSettingsValue("currency", "ranksrectype");

            CurrencyModel rankCurrency = null;
            CurrencyModel rankPointsCurrency = null;
            if (!string.IsNullOrEmpty(rankName))
            {
                if (rankAccumulationType == 1)
                {
                    rankCurrency = new CurrencyModel()
                    {
                        Name = rankName.Equals("Points") ? "Hours" : rankName,
                        SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(rankName.Equals("Points") ? "Hours" : rankName),
                        AcquireInterval = 60,
                        AcquireAmount = 1,
                        MaxAmount = rankMaxAmount,
                    };

                    if (rankInterval >= 0 && rankAmount >= 0)
                    {
                        rankPointsCurrency = new CurrencyModel()
                        {
                            Name = "Points",
                            SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("points"),
                            AcquireInterval = rankInterval,
                            AcquireAmount = rankAmount,
                            MaxAmount = rankMaxAmount,
                            OnFollowBonus = rankOnFollowBonus,
                            OnSubscribeBonus = rankOnSubBonus,
                            SubscriberBonus = rankSubBonus,
                            ModeratorBonus = rankSubBonus,
                            IsPrimary = true
                        };

                        ChannelSession.Settings.Currency[rankPointsCurrency.ID] = rankPointsCurrency;
                    }
                }
                else if (rankInterval >= 0 && rankAmount >= 0)
                {
                    rankCurrency = new CurrencyModel()
                    {
                        Name = rankName,
                        SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(rankName),
                        AcquireInterval = rankInterval,
                        AcquireAmount = rankAmount,
                        MaxAmount = rankMaxAmount,
                        OnFollowBonus = rankOnFollowBonus,
                        OnSubscribeBonus = rankOnSubBonus,
                        SubscriberBonus = rankSubBonus,
                        ModeratorBonus = rankSubBonus,
                        IsPrimary = true
                    };
                }
            }

            // Import Currency
            int currencyEnabled = this.GetIntSettingsValue("currency2", "enabled");
            string currencyName = this.GetSettingsValue("currency2", "name", "Currency");
            int currencyInterval = this.GetIntSettingsValue("currency2", "onlinepayinterval");
            int currencyAmount = this.GetIntSettingsValue("currency2", "activeuserbonus");
            int currencyMaxAmount = this.GetIntSettingsValue("currency2", "maxlimit");
            if (currencyMaxAmount <= 0)
            {
                currencyMaxAmount = int.MaxValue;
            }
            int currencyOnFollowBonus = this.GetIntSettingsValue("currency2", "onfollowbonus");
            int currencyOnSubBonus = this.GetIntSettingsValue("currency2", "onsubbonus");
            int currencySubBonus = this.GetIntSettingsValue("currency2", "subbonus");
            string currencyCommand = this.GetSettingsValue("currency2", "command", "");
            string currencyCommandResponse = this.GetSettingsValue("currency2", "response", "");

            CurrencyModel currency = null;
            if (!string.IsNullOrEmpty(currencyName) && currencyInterval >= 0 && currencyAmount >= 0)
            {
                currency = new CurrencyModel()
                {
                    Name = currencyName,
                    SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(currencyName),
                    AcquireInterval = currencyInterval,
                    AcquireAmount = currencyAmount,
                    MaxAmount = currencyMaxAmount,
                    OnFollowBonus = currencyOnFollowBonus,
                    OnSubscribeBonus = currencyOnSubBonus,
                    IsPrimary = true
                };
                ChannelSession.Settings.Currency[currency.ID] = currency;

                if (!string.IsNullOrEmpty(currencyCommand) && !string.IsNullOrEmpty(currencyCommandResponse))
                {
                    currencyCommandResponse = currencyCommandResponse.Replace("$points2", "$" + currency.UserAmountSpecialIdentifier);
                    currencyCommandResponse = currencyCommandResponse.Replace("$currencyname2", currency.Name);
                    this.Commands.Add(new ScorpBotCommandModel(currencyCommand, currencyCommandResponse));
                }
            }

            if (rankCurrency != null)
            {
                ChannelSession.Settings.Currency[rankCurrency.ID] = rankCurrency;

                foreach (ScorpBotRankModel rank in this.Ranks)
                {
                    rankCurrency.Ranks.Add(new RankModel(rank.Name, rank.Amount));
                }

                if (!string.IsNullOrEmpty(rankCommand) && !string.IsNullOrEmpty(rankCommandResponse))
                {
                    rankCommandResponse = rankCommandResponse.Replace(" / Raids: $raids", "");
                    rankCommandResponse = rankCommandResponse.Replace("$rank", "$" + rankCurrency.UserRankNameSpecialIdentifier);
                    rankCommandResponse = rankCommandResponse.Replace("$points", "$" + rankCurrency.UserAmountSpecialIdentifier);
                    rankCommandResponse = rankCommandResponse.Replace("$currencyname", rankCurrency.Name);
                    this.Commands.Add(new ScorpBotCommandModel(rankCommand, rankCommandResponse));
                }

                if (!string.IsNullOrEmpty(rankUpCommand))
                {
                    rankUpCommand = rankUpCommand.Replace("$rank", "$" + rankCurrency.UserRankNameSpecialIdentifier);
                    rankUpCommand = rankUpCommand.Replace("$points", "$" + rankCurrency.UserAmountSpecialIdentifier);
                    rankUpCommand = rankUpCommand.Replace("$currencyname", rankCurrency.Name);

                    ScorpBotCommandModel scorpCommand = new ScorpBotCommandModel("rankup", rankUpCommand);
                    ChatCommand chatCommand = new ChatCommand(scorpCommand);

                    CustomCommand miuRankUpCommand = new CustomCommand("User Rank Changed");
                    miuRankUpCommand.Actions.AddRange(chatCommand.Actions);
                    rankCurrency.RankChangedCommand = miuRankUpCommand;
                }
            }

            foreach (ScorpBotCommandModel command in this.Commands)
            {
                command.ProcessData(currency, rankCurrency);
                ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));
            }

            foreach (ScorpBotTimerModel timer in this.Timers)
            {
                ChannelSession.Settings.TimerCommands.Add(new TimerCommand(timer));
            }

            foreach (string quote in this.Quotes)
            {
                ChannelSession.Settings.Quotes.Add(new UserQuoteViewModel(quote, DateTimeOffset.MinValue, null));
            }

            if (ChannelSession.Settings.Quotes.Count > 0)
            {
                ChannelSession.Settings.QuotesEnabled = true;
            }

            if (this.GetBoolSettingsValue("settings", "filtwordsen"))
            {
                foreach (string filteredWord in this.FilteredWords)
                {
                    ChannelSession.Settings.FilteredWords.Add(filteredWord);
                }
                ChannelSession.Settings.ModerationFilteredWordsExcempt = this.GetUserRoleSettingsValue("settings", "FilteredWordsPerm");
            }

            if (this.GetBoolSettingsValue("settings", "chatcapschecknowarnregs"))
            {
                ChannelSession.Settings.ModerationChatTextExcempt = UserRoleEnum.User;
            }
            else if (this.GetBoolSettingsValue("settings", "chatcapschecknowarnsubs"))
            {
                ChannelSession.Settings.ModerationChatTextExcempt = UserRoleEnum.Subscriber;
            }
            else if (this.GetBoolSettingsValue("settings", "chatcapschecknowarnmods"))
            {
                ChannelSession.Settings.ModerationChatTextExcempt = UserRoleEnum.Mod;
            }
            else
            {
                ChannelSession.Settings.ModerationChatTextExcempt = UserRoleEnum.Streamer;
            }

            ChannelSession.Settings.ModerationCapsBlockIsPercentage = !this.GetBoolSettingsValue("settings", "chatcapsfiltertype");
            if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
            {
                ChannelSession.Settings.ModerationCapsBlockCount = this.GetIntSettingsValue("settings", "chatperccaps");
            }
            else
            {
                ChannelSession.Settings.ModerationCapsBlockCount = this.GetIntSettingsValue("settings", "chatmincaps");
            }

            ChannelSession.Settings.ModerationBlockLinks = this.GetBoolSettingsValue("settings", "chatlinkalertsdel");
            ChannelSession.Settings.ModerationBlockLinksExcempt = this.GetUserRoleSettingsValue("settings", "chatlinkalertsdelperm");

            foreach (ScorpBotViewerModel viewer in this.Viewers)
            {
                UserDataModel userData = new UserDataModel(viewer);
                ChannelSession.Settings.AddUserData(userData);

                UserViewModel user = new UserViewModel(userData);
                if (rankPointsCurrency != null)
                {
                    rankPointsCurrency.SetAmount(userData, (int)viewer.RankPoints);
                }

                if (rankCurrency != null)
                {
                    rankCurrency.SetAmount(userData, (rankPointsCurrency != null) ? (int)viewer.Hours : (int)viewer.RankPoints);
                }

                if (currency != null)
                {
                    currency.SetAmount(userData, (int)viewer.Currency);
                }
            }
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

        public UserRoleEnum GetUserRoleSettingsValue(string key, string value)
        {
            switch (this.GetIntSettingsValue(key, value))
            {
                case 0: return UserRoleEnum.User;
                case 1: return UserRoleEnum.Subscriber;
                case 2: return UserRoleEnum.Mod;
                case 3:
                case 4:
                default:
                    return UserRoleEnum.Streamer;
            }
        }
    }
}
