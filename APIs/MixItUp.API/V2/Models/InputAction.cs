namespace MixItUp.API.V2.Models
{
    public class InputAction : ActionBase
    {
        public string Key { get; set; }
        public string Mouse { get; set; }
        public string ActionType { get; set; }
        public bool Shift { get; set; }
        public bool Control { get; set; }
        public bool Alt { get; set; }
    }
}
