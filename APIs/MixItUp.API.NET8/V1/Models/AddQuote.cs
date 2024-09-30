using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class AddQuote
    {
        [Required]
        [DataMember]
        public string QuoteText { get; set; }
    }
}
