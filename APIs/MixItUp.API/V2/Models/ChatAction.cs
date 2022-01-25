namespace MixItUp.API.V2.Models
{
    public class ChatAction : ActionBase
    {
        public string ChatText { get; set; }
        public bool SendAsStreamer { get; set; }
        public bool IsWhisper { get; set; }
        public string WhisperUserName { get; set; }
    }
}
