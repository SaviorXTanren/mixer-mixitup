using System;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class Currency
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
