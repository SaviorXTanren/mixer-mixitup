using System;
using System.Collections.Generic;
using System.Text;

namespace MixItUp.API.Models
{
    public class MixPlayBroadcastGroup : MixPlayBroadcastTargetBase
    {
        public string GroupID { get; set; }

        public MixPlayBroadcastGroup(string groupID)
        {
            GroupID = groupID;
        }

        public override string ScopeString()
        {
            return $"group:{GroupID}";
        }
    }
}
