using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class UpdateInventoryAmount
    {
        [Required]
        [DataMember]
        public int Amount { get; set; }
    }
}
