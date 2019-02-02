using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MixItUp.API.Models
{
    public class MixPlayStatus
    {
        [DataMember]
        public bool IsConnected { get; set; }

        [DataMember]
        public string GameName { get; set; }
    }
}
