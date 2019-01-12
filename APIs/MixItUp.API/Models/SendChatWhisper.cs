using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class SendChatWhisper
    {
        [Required]
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }
    }
}
