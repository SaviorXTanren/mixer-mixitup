using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class NewUser
    {
        [Required]
        [DataMember]
        public string Platform { get; set; }

        [Required]
        [DataMember]
        public string Username { get; set; }
    }
}
