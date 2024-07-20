using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class TextToSpeechActionModel : ActionModelBase
    {
        [DataMember]
        public TextToSpeechProviderType ProviderType { get; set; } = TextToSpeechProviderType.ResponsiveVoice;

        [DataMember]
        public string OutputDevice { get; set; }
        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Voice { get; set; }

        [DataMember]
        public int Volume { get; set; }
        [DataMember]
        public int Pitch { get; set; }
        [DataMember]
        public int Rate { get; set; }

        [DataMember]
        public bool SSML { get; set; }

        [DataMember]
        public bool WaitForFinish { get; set; }

        public TextToSpeechActionModel(TextToSpeechProviderType providerType, string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
            : base(ActionTypeEnum.TextToSpeech)
        {
            this.ProviderType = providerType;
            this.OutputDevice = outputDevice;
            this.OverlayEndpointID = overlayEndpointID;
            this.Text = text;
            this.Voice = voice;
            this.Volume = volume;
            this.Pitch = pitch;
            this.Rate = rate;
            this.SSML = ssml;
            this.WaitForFinish = waitForFinish;
        }

        [Obsolete]
        public TextToSpeechActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string message = await ReplaceStringWithSpecialModifiers(this.Text, parameters);
            foreach (ITextToSpeechService service in ServiceManager.GetAll<ITextToSpeechService>())
            {
                if (service.ProviderType == this.ProviderType)
                {
                    await service.Speak(this.OutputDevice, this.OverlayEndpointID, message, this.Voice, this.Volume, this.Pitch, this.Rate, this.SSML, this.WaitForFinish);
                    break;
                }
            }
        }
    }
}
