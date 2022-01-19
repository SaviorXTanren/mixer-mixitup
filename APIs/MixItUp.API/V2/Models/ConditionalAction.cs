using System;
using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class ConditionalAction : ActionBase
    {
        public bool CaseSensitive { get; set; }
        public string Operator { get; set; }
        public bool RepeatWhileTrue { get; set; }
        public List<ConditionalClause> Clauses { get; set; } = new List<ConditionalClause>();
        public Guid CommandID { get; set; }
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }
}
