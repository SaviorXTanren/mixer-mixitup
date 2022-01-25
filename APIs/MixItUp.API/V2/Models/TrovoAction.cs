using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class TrovoAction : ActionBase
    {
        public string ActionType { get; set; }
        public string Username { get; set; }
        public string RoleName { get; set; }
        public int Amount { get; set; }
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }
}
