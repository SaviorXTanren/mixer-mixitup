using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MixItUp.API.Models
{
    public class MixPlayUser
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public List<string> ParticipantIDs { get; set; } = new List<string>();

    }
}
