using ExcelDataReader;
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
    }
}
