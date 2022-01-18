namespace MixItUp.API.V2.Models
{
    public class ThresholdRequirement : CommandRequirement
    {
        public int Amount { get; set; }
        public int TimeSpan { get; set; }
        public bool RunForEachUser { get; set; }
    }
}
