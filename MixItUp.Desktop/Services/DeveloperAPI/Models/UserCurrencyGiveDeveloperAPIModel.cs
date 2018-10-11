using System.Runtime.Serialization;

namespace MixItUp.Desktop.Services.DeveloperAPI.Models
{
    [DataContract]
    public class UserCurrencyGiveDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string UsernameOrID { get; set; }
    }
}
