namespace MixItUp.API.V2.Models
{
    public class IFTTTAction : ActionBase
    {
        public string EventName { get; set; }
        public string EventValue1 { get; set; }
        public string EventValue2 { get; set; }
        public string EventValue3 { get; set; }
    }
}
