using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class Error
    {
        [DataMember]
        public string Message { get; set; }
    }
}
