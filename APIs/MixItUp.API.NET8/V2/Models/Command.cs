using System;

namespace MixItUp.API.V2.Models
{
    public enum CommandStateOptions
    {
        Disable = 0,
        Enable = 1,
        Toggle = 2,
    }

    public class Command
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsEnabled { get; set; }
        public bool Unlocked { get; set; }
        public string GroupName { get; set; }
    }
}
