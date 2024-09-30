using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class SendChatWhisper
    {
        [Required]
        [DataMember]
        public string Message { get; set; }

        [Required]
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }
    }
}
