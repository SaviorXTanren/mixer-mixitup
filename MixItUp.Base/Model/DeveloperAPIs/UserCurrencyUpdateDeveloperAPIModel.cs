using System.Runtime.Serialization;

namespace MixItUp.Base.Model.DeveloperAPIs
{
    [DataContract]
    public class UserCurrencyUpdateDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }
    }
}
