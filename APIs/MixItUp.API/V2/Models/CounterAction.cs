namespace MixItUp.API.V2.Models
{
    public class CounterAction : ActionBase
    {
        public string CounterName { get; set; }
        public string ActionType { get; set; }
        public string Amount { get; set; }
    }
}
