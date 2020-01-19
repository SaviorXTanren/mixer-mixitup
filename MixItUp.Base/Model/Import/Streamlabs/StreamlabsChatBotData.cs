using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotData
    {
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

        public StreamlabsChatBotData()
        {
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
                this.Viewers.Add(new StreamlabsChatBotViewer(value));
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
