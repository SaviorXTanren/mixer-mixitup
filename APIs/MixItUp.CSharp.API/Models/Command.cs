using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.CSharp.API.Models
{
    [DataContract]
    public class Command
    {
        [Required]
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }
    }
}
