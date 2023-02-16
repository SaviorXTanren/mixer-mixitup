using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class RunCommandParameters
    {
        [DataMember]
        public string Platform { get; set; }

        [DataMember]
        public string Arguments { get; set; } = null;

        [DataMember]
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = null;
    }
}
