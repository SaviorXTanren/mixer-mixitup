using System;
using System.Runtime.Serialization;

namespace MixItUp.CSharp.API.Models
{
    [DataContract]
    public class Currency
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }
}
