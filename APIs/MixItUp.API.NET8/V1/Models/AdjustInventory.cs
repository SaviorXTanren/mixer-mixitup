using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class AdjustInventory
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }
}
