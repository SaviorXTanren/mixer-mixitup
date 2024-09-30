using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class Error
    {
        [DataMember]
        public string Message { get; set; }
    }
}
