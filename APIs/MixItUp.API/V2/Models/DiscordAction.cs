namespace MixItUp.API.V2.Models
{
    public class DiscordAction : ActionBase
    {
        public string ActionType { get; set; }
        public string ChannelID { get; set; }
        public string MessageText { get; set; }
        public string FilePath { get; set; }
        public bool ShouldMuteDeafen { get; set; }
    }
}
