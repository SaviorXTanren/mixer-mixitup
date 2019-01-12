using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class SendChatMessage
    {
        [Required]
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }
    }
}
