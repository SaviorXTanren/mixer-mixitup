using System;

namespace MixItUp.API.V2.Models
{
    public class RankRequirement : CommandRequirement
    {
        public Guid RankSystemID { get; set; }
        public string RankName { get; set; }
        public string MatchType { get; set; }
    }
}
