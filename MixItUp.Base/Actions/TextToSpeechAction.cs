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
            "Bosnian Male",
            "Brazilian Portuguese Female",
            "Catalan Male",
            "Chinese (Hong Kong) Female",
            "Chinese Female",
            "Chinese Taiwan Female",
            "Croatian Male",
            "Czech Female",
            "Czech Male",
            "Danish Female",
            "Danish Male",
            "Deutsch Female",
            "Dutch Female",
            "Esperanto Male",
            "Fallback UK Female",
            "Finnish Female",
            "Finnish Male",
            "French Female",
            "Greek Female",
            "Greek Male",
            "Hindi Female",
            "Hungarian Female",
            "Hungarian Male",
            "Icelandic Male",
            "Indonesian Female",
            "Italian Female",
            "Japanese Female",
            "Korean Female",
            "Latin Female",
            "Latin Male",
            "Latvian Male",
            "Macedonian Male",
            "Moldavian Male",
            "Montenegrin Male",
            "Norwegian Female",
            "Norwegian Male",
            "Polish Female",
            "Portuguese Female",
            "Romanian Male",
            "Russian Female",
            "Serbian Male",
            "Serbo-Croatian Male",
            "Slovak Female",
            "Slovak Male",
            "Spanish Female",
            "Spanish Latin American Female",
            "Swahili Male",
            "Swedish Female",
            "Swedish Male",
            "Tamil Male",
            "Thai Female",
            "Turkish Female",
            "UK English Female",
            "UK English Male",
            "US English Female",
            "US English Male",
            "Vietnamese Male",
            "Welsh Male"
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
            IOverlayService overlay = ChannelSession.Services.OverlayServers.GetOverlay(ChannelSession.Services.OverlayServers.DefaultOverlayName);
            if (overlay != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.SpeechText, user, arguments);
                await overlay.SendTextToSpeech(new OverlayTextToSpeech() { Text = message, Voice = this.Voice, Volume = this.Volume, Pitch = this.Pitch, Rate = this.Rate });
            }
        }
    }
}
