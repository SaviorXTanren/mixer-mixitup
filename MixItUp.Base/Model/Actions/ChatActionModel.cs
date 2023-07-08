using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ChatActionModel : ActionModelBase
    {
        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }
        [DataMember]
        public string WhisperUserName { get; set; }

        public ChatActionModel(string chatText, bool sendAsStreamer = false, bool isWhisper = false, string whisperUserName = null)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = chatText;
            this.SendAsStreamer = sendAsStreamer;
            this.IsWhisper = isWhisper;
            this.WhisperUserName = whisperUserName;
        }

        [Obsolete]
        public ChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string message = await ReplaceStringWithSpecialModifiers(this.ChatText, parameters);
            if (this.IsWhisper)
            {
                string whisperUserName = parameters.User.Username;
                if (!string.IsNullOrEmpty(this.WhisperUserName))
                {
                    whisperUserName = await ReplaceStringWithSpecialModifiers(this.WhisperUserName, parameters);
                }
                await ServiceManager.Get<ChatService>().Whisper(whisperUserName, parameters.Platform, message, this.SendAsStreamer);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(message, parameters, this.SendAsStreamer);
            }
        }
    }
}
