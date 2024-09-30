using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayResponsiveVoiceTextToSpeechV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = string.Empty;
        public static readonly string DefaultCSS = string.Empty;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Voice { get; set; }

        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Rate { get; set; }

        [DataMember]
        public bool WaitForFinish { get; set; }

        public OverlayResponsiveVoiceTextToSpeechV3Model(string text, string voice, double volume, double pitch, double rate, bool waitForFinish)
        {
            this.ID = Guid.NewGuid();
            this.Text = text;
            this.Voice = voice;
            this.Volume = volume;
            this.Pitch = pitch;
            this.Rate = rate;
            this.WaitForFinish = waitForFinish;
        }
    }
}
