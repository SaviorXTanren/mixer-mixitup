using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.DeveloperAPIs
{
    [DataContract]
    public class ChatMessageDeveloperAPIModel
    {
        [Required]
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }
    }
}
