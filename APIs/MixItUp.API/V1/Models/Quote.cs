using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class Quote
    {
        [Required]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public string GameName { get; set; }

        [DataMember]
        public string QuoteText { get; set; }
    }
}
