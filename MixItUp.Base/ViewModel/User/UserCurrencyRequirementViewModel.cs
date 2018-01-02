using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserCurrencyRequirementViewModel
    {
        [DataMember]
        public string CurrencyName { get; set; }

        [DataMember]
        public int RequiredAmount { get; set; }

        [DataMember]
        public UserRankViewModel RequiredRank { get; set; }

        public UserCurrencyRequirementViewModel() { }

        public UserCurrencyRequirementViewModel(UserCurrencyViewModel currency, int amount)
        {
            this.CurrencyName = currency.Name;
            this.RequiredAmount = amount;
        }

        public UserCurrencyRequirementViewModel(UserCurrencyViewModel currency, UserRankViewModel rank)
        {
            this.CurrencyName = currency.Name;
            this.RequiredRank = rank;
        }

        public UserCurrencyViewModel GetCurrency()
        {
            if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyName))
            {
                return ChannelSession.Settings.Currencies[this.CurrencyName];
            }
            return null;
        }
    }
}
