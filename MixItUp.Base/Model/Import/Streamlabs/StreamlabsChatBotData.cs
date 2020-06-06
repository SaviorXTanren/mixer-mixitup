using ExcelDataReader;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotData
    {
        public static async Task<StreamlabsChatBotData> GatherStreamlabsChatBotSettings(StreamingPlatformTypeEnum platform, string filePath)
        {
            StreamlabsChatBotData data = new StreamlabsChatBotData(platform);
            await Task.Run(() =>
            {
                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();

                            if (result.Tables.Contains("Commands"))
                            {
                                data.AddCommands(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Commands"]));
                            }
                            if (result.Tables.Contains("Timers"))
                            {
                                data.AddTimers(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Timers"]));
                            }
                            if (result.Tables.Contains("Quotes"))
                            {
                                data.AddQuotes(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Quotes"]));
                            }
                            if (result.Tables.Contains("Extra Quotes"))
                            {
                                data.AddQuotes(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Extra Quotes"]));
                            }
                            if (result.Tables.Contains("Ranks"))
                            {
                                data.AddRanks(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Ranks"]));
                            }
                            if (result.Tables.Contains("Currency"))
                            {
                                data.AddViewers(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Currency"]));
                            }
                            if (result.Tables.Contains("Events"))
                            {
                                data.AddEvents(StreamlabsChatBotData.ReadRowsFromTable(result.Tables["Events"]));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    data = null;
                }
            });
            return data;
        }

        private static List<List<string>> ReadRowsFromTable(DataTable table)
        {
            List<List<string>> results = new List<List<string>>();
            for (int i = 1; i < table.Rows.Count; i++)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < table.Rows[i].ItemArray.Length; j++)
                {
                    row.Add(table.Rows[i].ItemArray[j].ToString());
                }
                results.Add(row);
            }
            return results;
        }

        [DataMember]
        StreamingPlatformTypeEnum Platform { get; set; }
        [DataMember]
        public List<StreamlabsChatBotCommand> Commands { get; set; }
        [DataMember]
        public List<StreamlabsChatBotTimer> Timers { get; set; }
        [DataMember]
        public List<string> Quotes { get; set; }
        [DataMember]
        public List<StreamlabsChatBotRank> Ranks { get; set; }
        [DataMember]
        public List<StreamlabsChatBotViewer> Viewers { get; set; }
        [DataMember]
        public List<StreamlabsChatBotEvent> Events { get; set; }

        public StreamlabsChatBotData(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;
            this.Commands = new List<StreamlabsChatBotCommand>();
            this.Timers = new List<StreamlabsChatBotTimer>();
            this.Quotes = new List<string>();
            this.Ranks = new List<StreamlabsChatBotRank>();
            this.Viewers = new List<StreamlabsChatBotViewer>();
            this.Events = new List<StreamlabsChatBotEvent>();
        }

        public async Task ImportSettings()
        {
            CurrencyModel rank = new CurrencyModel()
            {
                Name = "Rank",
                SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("rank"),
                AcquireInterval = 60,
                AcquireAmount = 1,
                IsPrimary = true
            };

            foreach (StreamlabsChatBotRank slrank in this.Ranks)
            {
                rank.Ranks.Add(new RankModel(slrank.Name, slrank.Requirement));
            }

            CurrencyModel currency = new CurrencyModel()
            {
                Name = "Points",
                SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("points"),
                AcquireInterval = 1,
                AcquireAmount = 1,
                IsPrimary = true
            };

            ChannelSession.Settings.Currency[rank.ID] = rank;
            ChannelSession.Settings.Currency[currency.ID] = currency;

            this.AddCurrencyRankCommands(rank);
            this.AddCurrencyRankCommands(currency);

            foreach (StreamlabsChatBotViewer viewer in this.Viewers)
            {
                try
                {
                    UserModel user = await ChannelSession.MixerUserConnection.GetUser(viewer.Name);
                    if (user != null)
                    {
                        viewer.ID = user.id;
                        UserDataModel userData = new UserDataModel(viewer);
                        ChannelSession.Settings.UserData[userData.ID] = userData;
                        rank.SetAmount(userData, viewer.Hours);
                        currency.SetAmount(userData, viewer.Points);
                    }
                }
                catch (Exception) { }
            }

            foreach (StreamlabsChatBotCommand command in this.Commands)
            {
                command.ProcessData(currency, rank);
                ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));
            }

            foreach (StreamlabsChatBotTimer timer in this.Timers)
            {
                StreamlabsChatBotCommand command = new StreamlabsChatBotCommand() { Command = timer.Name, Response = timer.Response, Enabled = timer.Enabled };
                command.ProcessData(currency, rank);
                ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));

                timer.Actions = command.Actions;

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
        }

        public void AddCommands(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Commands.Add(new StreamlabsChatBotCommand(value));
            }
        }

        public void AddTimers(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Timers.Add(new StreamlabsChatBotTimer(value));
            }
        }

        public void AddQuotes(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Quotes.Add(value[1]);
            }
        }

        public void AddRanks(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Ranks.Add(new StreamlabsChatBotRank(value));
            }
        }

        public void AddViewers(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Viewers.Add(new StreamlabsChatBotViewer(this.Platform, value));
            }
        }

        public void AddEvents(List<List<string>> values)
        {
            foreach (List<string> value in values)
            {
                this.Events.Add(new StreamlabsChatBotEvent(value));
            }
        }

        private void AddCurrencyRankCommands(CurrencyModel currency)
        {
            ChatCommand statusCommand = new ChatCommand("User " + currency.Name, currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
            string statusChatText = string.Empty;
            if (currency.IsRank)
            {
                statusChatText = string.Format("@$username is a ${0} with ${1} {2}!", currency.UserRankNameSpecialIdentifier, currency.UserAmountSpecialIdentifier, currency.Name);
            }
            else
            {
                statusChatText = string.Format("@$username has ${0} {1}!", currency.UserAmountSpecialIdentifier, currency.Name);
            }
            statusCommand.Actions.Add(new ChatAction(statusChatText));
            ChannelSession.Settings.ChatCommands.Add(statusCommand);

            ChatCommand addCommand = new ChatCommand("Add " + currency.Name, "add" + currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
            addCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername"));
            addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text {0}!", currency.Name)));
            ChannelSession.Settings.ChatCommands.Add(addCommand);

            ChatCommand addAllCommand = new ChatCommand("Add All " + currency.Name, "addall" + currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
            addAllCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToAllChatUsers, "$arg1text"));
            addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text {0}!", currency.Name)));
            ChannelSession.Settings.ChatCommands.Add(addAllCommand);

            if (!currency.IsRank)
            {
                ChatCommand giveCommand = new ChatCommand("Give " + currency.Name, "give" + currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
                giveCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername", deductFromUser: true));
                giveCommand.Actions.Add(new ChatAction(string.Format("@$username gave @$targetusername $arg2text {0}!", currency.Name)));
                ChannelSession.Settings.ChatCommands.Add(giveCommand);
            }
        }
    }
}
