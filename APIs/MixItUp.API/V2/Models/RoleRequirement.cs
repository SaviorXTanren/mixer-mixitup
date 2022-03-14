namespace MixItUp.API.V2.Models
{
    public class RoleRequirement : CommandRequirement
    {
        public string StreamingPlatform { get; set; }
        public string UserRole { get; set; }
        public int SubscriberTier { get; set; } = 1;
        public string PatreonBenefitID { get; set; }
    }
}
