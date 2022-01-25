namespace MixItUp.API.V2.Models
{
    public class CooldownRequirement : CommandRequirement
    {
        public string Type { get; set; }
        public int IndividualAmount { get; set; }
        public string GroupName { get; set; }
    }
}
