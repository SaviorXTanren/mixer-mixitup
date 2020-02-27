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
        public static readonly IEnumerable<string> AvailableVoices = new List<string>()
        {
            "Afrikaans Male",
            "Albanian Male",
            "Arabic Female",
            "Arabic Male",
            "Armenian Male",
            "Australian Female",
            "Australian Male",
            "Bosnian Female",
            "Bosnian Male",
            "Brazilian Portugese Male",
            "Brazilian Portuguese Female",
            "Catalan Male",
            "Chinese (Hong Kong) Female",
            "Chinese (Hong Kong) Male",
            "Chinese Female",
            "Chinese Male",
            "Chinese Taiwan Female",
            "Chinese Taiwan Male",
            "Croatian Female",
            "Croatian Male",
            "Czech Female",
            "Czeck Male",
            "Danish Female",
            "Danish Male",
            "Dutch Female",
            "Dutch Male",
            "Esperanto Male",
            "Finnish Female",
            "Finnish Male",
            "French Female",
            "French Male",
            "German Female",
            "German Male",
            "Greek Female",
            "Greek Male",
            "Hindi Female",
            "Hindi Male",
            "Hungarian Female",
            "Hungarian Male",
            "Icelandic Male",
            "Indonesian Female",
            "Indonesian Male",
            "Italian Female",
            "Italian Male",
            "Japanese Female",
            "Japanese Male",
            "Korean Female",
            "Korean Male",
            "Latin Female",
            "Latin Male",
            "Latvian Male",
            "Macedonian Male",
            "Moldavian Male",
            "Montenegrin Male",
            "Norwegian Female",
            "Norwegian Male",
            "Polish Female",
            "Polish Male",
            "Portugese Male",
            "Portuguese Female",
            "Romanian Female",
            "Romanian Male",
            "Russian Female",
            "Russian Male",
            "Serbian Female",
            "Serbian Male",
            "Serbo-Croatian Male",
            "Slovak Female",
            "Slovak Male",
            "Spanish Female",
            "Spanish Latin American Female",
            "Spanish Latin American Male",
            "Spanish Male",
            "Swahili Male",
            "Swedish Female",
            "Swedish Male",
            "Tamil Male",
            "Thai Female",
            "Turkish Female",
            "Turkish Male",
            "UK English Female",
            "UK English Male",
            "US English Female",
            "US English Male",
            "Vietnamese Female",
            "Vietnamese Male",
            "Welsh Male",
        };

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TextToSpeechAction.asyncSemaphore; } }

        [DataMember]
        public string SpeechText { get; set; }

        [DataMember]
        public string Voice { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [DataMember]
        public double Pitch { get; set; }

        [DataMember]
        public double Rate { get; set; }

        public TextToSpeechAction()
            : base(ActionTypeEnum.TextToSpeech)
        {
            this.Voice = "US English Male";
            this.Pitch = 1;
            this.Rate = 1;
            this.Volume = 1;
        }

        public TextToSpeechAction(string speechText, string voice, double volume, double pitch, double rate)
            : this()
        {
            this.SpeechText = speechText;
            this.Voice = voice;
            this.Volume = volume;
            this.Pitch = pitch;
            this.Rate = rate;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            IOverlayEndpointService overlay = ChannelSession.Services.Overlay.GetOverlay(ChannelSession.Services.Overlay.DefaultOverlayName);
            if (overlay != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.SpeechText, user, arguments);
                await overlay.SendTextToSpeech(new OverlayTextToSpeech() { Text = message, Voice = this.Voice, Volume = this.Volume, Pitch = this.Pitch, Rate = this.Rate });
            }
        }
    }
}
