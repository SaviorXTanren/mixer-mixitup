using System.Runtime.Serialization;

namespace MixItUp.Desktop.Services.DeveloperAPI.Models
{
    [DataContract]
    public class UserCurrencyUpdateDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
