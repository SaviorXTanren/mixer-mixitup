using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class OvrStreamAction : ActionBase
    {
        public string ActionType { get; set; }
        public string TitleName { get; set; }
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
