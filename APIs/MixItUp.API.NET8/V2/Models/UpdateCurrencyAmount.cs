using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class UpdateCurrencyAmount
    {
        [Required]
        [DataMember]
        public int Amount { get; set; }
    }
}
