namespace MixItUp.API.V2.Models
{
    public class SettingsRequirement : CommandRequirement
    {
        public bool DeleteChatMessageWhenRun { get; set; }
        public bool DontDeleteChatMessageWhenRun { get; set; }
        public bool ShowOnChatContextMenu { get; set; }
    }
}
