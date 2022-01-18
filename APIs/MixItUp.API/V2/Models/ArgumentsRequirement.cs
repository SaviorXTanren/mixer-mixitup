using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class ArgumentsRequirement : CommandRequirement
    {
        public List<ArgumentsRequirementItem> Items { get; set; } = new List<ArgumentsRequirementItem>();
    }
}
