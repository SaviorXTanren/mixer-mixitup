using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int? ViewingMinutes { get; set; }

        [DataMember]
        public List<Currency> CurrencyAmounts { get; set; } = new List<Currency>();
    }
}
