using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class AdjustCurrency
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
