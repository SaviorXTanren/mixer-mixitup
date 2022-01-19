using System;

namespace MixItUp.API.V2.Models
{
    public class CommandAction : ActionBase
    {
        public string ActionType { get; set; }
        public Guid CommandID { get; set; }
        public string PreMadeType { get; set; }
        public string Arguments { get; set; }
        public bool WaitForCommandToFinish { get; set; }
        public string CommandGroupName { get; set; }
    }
}
