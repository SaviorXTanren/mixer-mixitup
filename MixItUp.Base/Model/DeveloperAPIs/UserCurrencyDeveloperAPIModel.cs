using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.DeveloperAPIs
{
    [DataContract]
    public class UserCurrencyDeveloperAPIModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public UserCurrencyDeveloperAPIModel() { }

        public UserCurrencyDeveloperAPIModel(UserCurrencyDataViewModel currencyData)
        {
            this.ID = currencyData.Currency.ID;
            this.Name = currencyData.Currency.Name;
            this.Amount = currencyData.Amount;
        }
    }
}
