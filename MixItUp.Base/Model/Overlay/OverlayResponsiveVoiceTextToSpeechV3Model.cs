using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
            this.Text = text;
            this.Voice = voice;
            this.Volume = volume;
            this.Pitch = pitch;
            this.Rate = rate;
            this.WaitForFinish = waitForFinish;
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Text), this.Text);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Text), this.Text);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Text), this.Text);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Voice), this.Voice);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Voice), this.Voice);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Voice), this.Voice);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Volume), this.Volume.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Volume), this.Volume.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Volume), this.Volume.ToString());

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Pitch), this.Pitch.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Pitch), this.Pitch.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Pitch), this.Pitch.ToString());

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Rate), this.Rate.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Rate), this.Rate.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Rate), this.Rate.ToString());

            item.HTML = ReplaceProperty(item.HTML, nameof(this.WaitForFinish), this.WaitForFinish.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.WaitForFinish), this.WaitForFinish.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.WaitForFinish), this.WaitForFinish.ToString());

            return item;
        }
    }
}
