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

        public UserCurrencyDeveloperAPIModel(UserCurrencyViewModel currencyData, int amount)
        {
            this.ID = currencyData.ID;
            this.Name = currencyData.Name;
            this.Amount = amount;
        }
    }
}
