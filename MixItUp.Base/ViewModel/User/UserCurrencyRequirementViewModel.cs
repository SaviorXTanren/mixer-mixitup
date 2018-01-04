using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public string RankName { get; set; }
        [DataMember]
        public bool MustEqual { get; set; }

        public UserCurrencyRequirementViewModel() { }

        public UserCurrencyRequirementViewModel(UserCurrencyViewModel currency, int amount)
        {
            this.CurrencyName = currency.Name;
            this.RequiredAmount = amount;
        }

        public UserCurrencyRequirementViewModel(UserCurrencyViewModel currency, UserRankViewModel rank, bool mustEqual = false)
        {
            this.CurrencyName = currency.Name;
            this.RankName = rank.Name;
            this.MustEqual = mustEqual;
        }

        public UserRankViewModel RequiredRank
        {
            get
            {
                UserCurrencyViewModel currency = this.GetCurrency();
                if (currency != null)
                {
                    UserRankViewModel rank = currency.Ranks.FirstOrDefault(r => r.Name.Equals(this.RankName));
                    if (rank != null)
                    {
                        return rank;
                    }
                }
                return new UserRankViewModel("No Rank", 0);
            }
        }

        public UserCurrencyViewModel GetCurrency()
        {
            if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyName))
            {
                return ChannelSession.Settings.Currencies[this.CurrencyName];
            }
            return null;
        }

        public bool DoesUserMeetRequirement(UserDataViewModel userData)
        {
            UserCurrencyViewModel currency = this.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            UserCurrencyDataViewModel userCurrencyData = userData.GetCurrency(currency);
            if (userCurrencyData.Amount < this.RequiredAmount)
            {
                return false;
            }

            UserRankViewModel rank = this.RequiredRank;
            if (rank != null)
            {
                if (userCurrencyData.Amount < rank.MinimumPoints)
                {
                    return false;
                }

                if (this.MustEqual && userCurrencyData.GetRank() != rank)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task SendCurrencyNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", this.RequiredAmount, this.CurrencyName));
            }
        }

        public async Task SendRankNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required rank of {0} ({1} {2}) to do this",
                    this.RequiredRank.Name, this.RequiredRank.MinimumPoints, this.CurrencyName));
            }
        }
    }
}
