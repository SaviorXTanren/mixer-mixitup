using System.Runtime.Serialization;

namespace MixItUp.API.Models
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
