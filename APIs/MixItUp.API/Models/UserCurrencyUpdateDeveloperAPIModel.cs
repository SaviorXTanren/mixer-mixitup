using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class UserCurrencyUpdateDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
