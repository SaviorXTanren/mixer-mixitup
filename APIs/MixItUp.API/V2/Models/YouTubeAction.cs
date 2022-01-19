using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class YouTubeAction : ActionBase
    {
        public string ActionType { get; set; }
        public int AdBreakLength { get; set; }
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }
}
