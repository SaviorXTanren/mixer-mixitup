using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum DiscordActionTypeEnum
    {
        SendMessage,
        MuteSelf,
        DeafenSelf,
    }

    [DataContract]
    public class DiscordActionModel : ActionModelBase
    {
        public static DiscordActionModel CreateForChatMessage(DiscordChannel channel, string message, string filePath) { return new DiscordActionModel(DiscordActionTypeEnum.SendMessage) { ChannelID = channel.ID, MessageText = message, FilePath = filePath }; }

        public static DiscordActionModel CreateForMuteSelf(bool mute) { return new DiscordActionModel(DiscordActionTypeEnum.MuteSelf) { ShouldMuteDeafen = mute }; }

        public static DiscordActionModel CreateForDeafenSelf(bool deafen) { return new DiscordActionModel(DiscordActionTypeEnum.DeafenSelf) { ShouldMuteDeafen = deafen }; }

        [DataMember]
        public DiscordActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ChannelID { get; set; }

        [DataMember]
        public string MessageText { get; set; }
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public bool ShouldMuteDeafen { get; set; }

        private DiscordChannel channel;

        public DiscordActionModel(DiscordActionTypeEnum actionType)
            : base(ActionTypeEnum.Discord)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public DiscordActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == DiscordActionTypeEnum.SendMessage)
            {
                if (this.channel == null)
                {
                    this.channel = await ServiceManager.Get<DiscordService>().GetChannel(this.ChannelID);
                }

                if (this.channel != null)
                {
                    string message = await ReplaceStringWithSpecialModifiers(this.MessageText, parameters);
                    string filePath = await ReplaceStringWithSpecialModifiers(this.FilePath, parameters);

                    if (!string.IsNullOrEmpty(filePath) && !ServiceManager.Get<IFileService>().IsURLPath(filePath) && !ServiceManager.Get<IFileService>().FileExists(filePath))
                    {
                        Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - Discord Action - File does not exist: {filePath}");
                    }

                    await ServiceManager.Get<DiscordService>().CreateMessage(this.channel, message, filePath);
                }
            }
            else if (this.ActionType == DiscordActionTypeEnum.MuteSelf)
            {
                await ServiceManager.Get<DiscordService>().MuteServerMember(ServiceManager.Get<DiscordService>().Server, ServiceManager.Get<DiscordService>().User, this.ShouldMuteDeafen);
            }
            else if (this.ActionType == DiscordActionTypeEnum.DeafenSelf)
            {
                await ServiceManager.Get<DiscordService>().DeafenServerMember(ServiceManager.Get<DiscordService>().Server, ServiceManager.Get<DiscordService>().User, this.ShouldMuteDeafen);
            }
        }
    }
}
