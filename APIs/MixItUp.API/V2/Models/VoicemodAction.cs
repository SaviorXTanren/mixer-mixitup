namespace MixItUp.API.V2.Models
{
    public class VoicemodAction : ActionBase
    {
        public string ActionType { get; set; }
        public bool State { get; set; }
        public string VoiceID { get; set; }
        public string RandomVoiceType { get; set; }
        public string SoundFileName { get; set; }
    }
}
