using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class GiveUserCurrency
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string UsernameOrID { get; set; }
    }
}
