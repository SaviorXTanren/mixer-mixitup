using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class Error
    {
        [DataMember]
        public string Message { get; set; }
    }
}
