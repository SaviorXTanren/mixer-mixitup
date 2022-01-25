using System;

namespace MixItUp.API.V2.Models
{
    public class CurrencyRequirement : CommandRequirement
    {
        public Guid CurrencyID { get; set; }
        public string RequirementType { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
    }
}
