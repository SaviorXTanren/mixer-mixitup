using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.DeveloperAPIs
{
    [DataContract]
    public class UserDeveloperAPIModel
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public List<UserCurrencyDeveloperAPIModel> CurrencyAmounts { get; set; }

        public UserDeveloperAPIModel()
        {
            this.CurrencyAmounts = new List<UserCurrencyDeveloperAPIModel>();
        }

        public UserDeveloperAPIModel(UserDataViewModel userData)
            : this()
        {
            this.ID = userData.ID;
            this.UserName = userData.UserName;
            this.ViewingMinutes = userData.ViewingMinutes;
            foreach (UserCurrencyDataViewModel currencyData in userData.CurrencyAmounts.Values)
            {
                this.CurrencyAmounts.Add(new UserCurrencyDeveloperAPIModel(currencyData));
            }
        }
    }
}
