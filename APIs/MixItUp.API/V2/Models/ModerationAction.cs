namespace MixItUp.API.V2.Models
{
    public class ModerationAction : ActionBase
    {
        public string ActionType { get; set; }
        public string TargetUsername { get; set; }
        public string TimeoutAmount { get; set; }
        public string ModerationReason { get; set; }
    }
}
