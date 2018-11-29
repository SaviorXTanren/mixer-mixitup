using System;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class CurrencyAmount
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }
}
