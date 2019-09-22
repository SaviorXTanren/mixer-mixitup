using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum StreamingPlatformActionType
    {
        Host,
        Poll
    }

    [DataContract]
    public class StreamingPlatformAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamingPlatformAction.asyncSemaphore; } }

        public StreamingPlatformActionType ActionType { get; set; }

        [DataMember]
        public string PollQuestion { get; set; }
        [DataMember]
        public List<string> PollAnswers { get; set; }
        [DataMember]
        public uint PollLength { get; set; }
        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public string HostChannelName { get; set; }

        [JsonIgnore]
        private IEnumerable<string> lastArguments = null;

        public CommandBase Command { get { return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); } }

        public static StreamingPlatformAction CreateHostAction(string channelName)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.Host);
            action.HostChannelName = channelName;
            return action;
        }

        public static StreamingPlatformAction CreatePollAction(string question, uint length, IEnumerable<string> answers, CommandBase command)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.Poll);
            action.PollQuestion = question;
            action.PollLength = length;
            action.PollAnswers = new List<string>(answers);
            action.CommandID = (command != null) ? command.ID : Guid.Empty;
            return action;
        }

        public StreamingPlatformAction() : base(ActionTypeEnum.StreamingPlatform) { }

        private StreamingPlatformAction(StreamingPlatformActionType type)
            : this()
        {
            this.ActionType = type;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            this.lastArguments = arguments;
            if (this.ActionType == StreamingPlatformActionType.Poll)
            {
                if (ChannelSession.Services.Chat != null)
                {
                    string pollQuestion = await this.ReplaceStringWithSpecialModifiers(this.PollQuestion, user, arguments);
                    List<string> pollAnswers = new List<string>();
                    foreach (string pollAnswer in this.PollAnswers)
                    {
                        pollAnswers.Add(await this.ReplaceStringWithSpecialModifiers(pollAnswer, user, arguments));
                    }

                    if (this.CommandID != Guid.Empty)
                    {
                        ChannelSession.Services.Chat.OnPollEndOccurred += Chat_OnPollEnd;
                    }
                    await ChannelSession.Services.Chat.StartPoll(pollQuestion, pollAnswers, this.PollLength);
                }
            }
            else if (this.ActionType == StreamingPlatformActionType.Host)
            {
                string hostChannelName = await this.ReplaceStringWithSpecialModifiers(this.HostChannelName, user, arguments);
                ChannelModel channel = await ChannelSession.MixerStreamerConnection.GetChannel(hostChannelName);
                if (channel != null)
                {
                    await ChannelSession.MixerStreamerConnection.SetHostChannel(ChannelSession.MixerChannel, channel);
                }
            }
        }


        private void Chat_OnPollEnd(object sender, Dictionary<string, uint> results)
        {
            ChannelSession.Services.Chat.OnPollEndOccurred -= Chat_OnPollEnd;
            Task.Run(async () =>
            {
                try
                {
                    if (results.Count > 0)
                    {
                        var winner = results.OrderByDescending(r => r.Value).First();
                        if (winner.Value > 0)
                        {
                            CommandBase command = ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                            if (command != null)
                            {
                                this.extraSpecialIdentifiers["pollresultanswer"] = winner.Key;
                                this.extraSpecialIdentifiers["pollresulttotal"] = winner.Value.ToString();
                                await command.Perform(arguments: this.lastArguments, extraSpecialIdentifiers: this.GetExtraSpecialIdentifiers());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }
    }
}
