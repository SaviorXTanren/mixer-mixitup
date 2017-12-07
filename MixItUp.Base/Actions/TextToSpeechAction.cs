using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class TextToSpeechAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TextToSpeechAction.asyncSemaphore; } }

        [DataMember]
        public string SpeechText { get; set; }

        [DataMember]
        public SpeechRate SpeechRate { get; set; }
        
        [DataMember]
        public SpeechVolume SpeechVolume { get; set; }

        public TextToSpeechAction() : base(ActionTypeEnum.TextToSpeech) { }

        public TextToSpeechAction(string speechText, SpeechRate speechRate, SpeechVolume speechVolume)
            : this()
        {
            this.SpeechText = speechText;
            this.SpeechRate = speechRate;
            this.SpeechVolume = speechVolume;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.SpeechText, user, arguments);
                if (ChannelSession.Services.TextToSpeechService != null)
                {
                    await ChannelSession.Services.TextToSpeechService.SayText(message, this.SpeechRate, this.SpeechVolume);
                }
            }
        }
    }
}
