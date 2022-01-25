namespace MixItUp.API.V2.Models
{
    public class GameQueueAction : ActionBase
    {
        public string ActionType { get; set; }
        public string RoleRequirement { get; set; }
        public string TargetUsername { get; set; }
    }
}
