using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class AdjustCurrency
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
