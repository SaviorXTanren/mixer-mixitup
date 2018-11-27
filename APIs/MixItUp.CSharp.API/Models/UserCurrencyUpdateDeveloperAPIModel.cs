using System.Runtime.Serialization;

namespace MixItUp.CSharp.API.Models
{
    [DataContract]
    public class UserCurrencyUpdateDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
