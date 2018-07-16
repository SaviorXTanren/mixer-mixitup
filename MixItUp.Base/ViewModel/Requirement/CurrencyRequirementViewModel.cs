using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public enum CurrencyRequirementTypeEnum
    {
        [Name("No Currency Cost")]
        NoCurrencyCost,
        [Name("Minimum Only")]
        MinimumOnly,
        [Name("Minimum & Maximum")]
        MinimumAndMaximum,
        [Name("Required Amount")]
        RequiredAmount
    }

    [DataContract]
    public class CurrencyRequirementViewModel : IEquatable<CurrencyRequirementViewModel>
    {
        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public int RequiredAmount { get; set; }
        [DataMember]
        public int MaximumAmount { get; set; }
        [DataMember]
        public CurrencyRequirementTypeEnum RequirementType { get; set; }

        [DataMember]
        public string RankName { get; set; }
        [DataMember]
        public bool MustEqual { get; set; }

        public CurrencyRequirementViewModel()
        {
            this.RequirementType = CurrencyRequirementTypeEnum.RequiredAmount;
        }

        public CurrencyRequirementViewModel(UserCurrencyViewModel currency, int amount)
            : this(currency, CurrencyRequirementTypeEnum.RequiredAmount, amount)
        { }

        public CurrencyRequirementViewModel(UserCurrencyViewModel currency, int minimumAmount, int maximumAmount)
            : this(currency, CurrencyRequirementTypeEnum.MinimumAndMaximum, minimumAmount)
        {
            this.MaximumAmount = maximumAmount;
        }

        public CurrencyRequirementViewModel(UserCurrencyViewModel currency, CurrencyRequirementTypeEnum requirementType, int amount)
        {
            this.CurrencyID = currency.ID;
            this.RequiredAmount = amount;
            this.RequirementType = requirementType;
        }

        public CurrencyRequirementViewModel(UserCurrencyViewModel currency, UserRankViewModel rank, bool mustEqual = false)
        {
            this.CurrencyID = currency.ID;
            this.RankName = rank.Name;
            this.MustEqual = mustEqual;
        }

        [JsonIgnore]
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
                return UserCurrencyViewModel.NoRank;
            }
        }

        public UserCurrencyViewModel GetCurrency()
        {
            if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                return ChannelSession.Settings.Currencies[this.CurrencyID];
            }
            return null;
        }

        public bool TrySubtractAmount(UserDataViewModel userData) { return this.TrySubtractAmount(userData, this.RequiredAmount); }

        public bool TrySubtractAmount(UserDataViewModel userData, int amount)
        {
            if (this.DoesMeetCurrencyRequirement(amount))
            {
                UserCurrencyViewModel currency = this.GetCurrency();
                if (currency == null)
                {
                    return false;
                }

                UserCurrencyDataViewModel userCurrencyData = userData.GetCurrency(currency);
                if (userCurrencyData.Amount < amount)
                {
                    return false;
                }

                userCurrencyData.Amount -= amount;
                return true;
            }
            return false;
        }

        public bool DoesMeetCurrencyRequirement(UserDataViewModel userData)
        {
            if (userData.IsCurrencyRankExempt)
            {
                return true;
            }

            UserCurrencyViewModel currency = this.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            UserRankViewModel rank = this.RequiredRank;
            if (rank == null)
            {
                return false;
            }

            UserCurrencyDataViewModel userCurrencyData = userData.GetCurrency(currency);

            return this.DoesMeetCurrencyRequirement(userCurrencyData.Amount);
        }

        public bool DoesMeetCurrencyRequirement(int amount)
        {
            if (amount < this.RequiredAmount)
            {
                return false;
            }

            if (this.MaximumAmount > 0 && amount > this.MaximumAmount)
            {
                return false;
            }

            return true;
        }

        public bool DoesMeetRankRequirement(UserDataViewModel userData)
        {
            if (userData.IsCurrencyRankExempt)
            {
                return true;
            }

            UserCurrencyViewModel currency = this.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            UserRankViewModel rank = this.RequiredRank;
            if (rank == null)
            {
                return false;
            }

            UserCurrencyDataViewModel userCurrencyData = userData.GetCurrency(currency);
            if (userCurrencyData.Amount < rank.MinimumPoints)
            {
                return false;
            }

            if (this.MustEqual && userCurrencyData.GetRank() != rank)
            {
                return false;
            }

            return true;
        }

        public async Task SendCurrencyNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0}-{1} {2} to do this",
                        this.RequiredAmount, this.MaximumAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this",
                        this.RequiredAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
            }
        }

        public async Task SendRankNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required rank of {0} ({1} {2}) to do this",
                    this.RequiredRank.Name, this.RequiredRank.MinimumPoints, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CurrencyRequirementViewModel)
            {
                return this.Equals((CurrencyRequirementViewModel)obj);
            }
            return false;
        }

        public bool Equals(CurrencyRequirementViewModel other) { return this.CurrencyID.Equals(other.CurrencyID); }

        public override int GetHashCode() { return this.CurrencyID.GetHashCode(); }
    }
}
