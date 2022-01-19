namespace MixItUp.API.V2.Models
{
    public class TextToSpeechAction : ActionBase
    {
        public string Text { get; set; }
        public string Voice { get; set; }
        public int Volume { get; set; }
        public int Pitch { get; set; }
        public int Rate { get; set; }
    }
}
