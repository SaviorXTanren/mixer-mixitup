using System;
using System.Collections.Generic;

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
        public bool IsEmbedded { get; set; }
        public string GroupName { get; set; }
        public HashSet<string> Triggers { get; set; } = new HashSet<string>();
        public List<CommandRequirement> Requirements { get; set; } = new List<CommandRequirement>();
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }
}
