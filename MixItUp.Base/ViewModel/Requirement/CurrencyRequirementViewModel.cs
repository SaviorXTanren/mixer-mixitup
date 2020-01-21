using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
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

        public CurrencyRequirementViewModel(UserCurrencyModel currency, int amount)
            : this(currency, CurrencyRequirementTypeEnum.RequiredAmount, amount)
        { }

        public CurrencyRequirementViewModel(UserCurrencyModel currency, int minimumAmount, int maximumAmount)
            : this(currency, CurrencyRequirementTypeEnum.MinimumAndMaximum, minimumAmount)
        {
            this.MaximumAmount = maximumAmount;
        }

        public CurrencyRequirementViewModel(UserCurrencyModel currency, CurrencyRequirementTypeEnum requirementType, int amount)
        {
            this.CurrencyID = currency.ID;
            this.RequiredAmount = amount;
            this.RequirementType = requirementType;
        }

        public CurrencyRequirementViewModel(UserCurrencyModel currency, UserRankViewModel rank, bool mustEqual = false)
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
                UserCurrencyModel currency = this.GetCurrency();
                if (currency != null)
                {
                    UserRankViewModel rank = currency.Ranks.FirstOrDefault(r => r.Name.Equals(this.RankName));
                    if (rank != null)
                    {
                        return rank;
                    }
                }
                return UserCurrencyModel.NoRank;
            }
        }

        public UserCurrencyModel GetCurrency()
        {
            if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                return ChannelSession.Settings.Currencies[this.CurrencyID];
            }
            return null;
        }

        public bool TrySubtractAmount(UserDataViewModel userData) { return this.TrySubtractAmount(userData, this.RequiredAmount); }

        public bool TrySubtractMultiplierAmount(UserDataViewModel userData, int multiplier) { return this.TrySubtractAmount(userData, multiplier * this.RequiredAmount); }

        public bool TrySubtractAmount(UserDataViewModel userData, int amount)
        {
            if (this.DoesMeetCurrencyRequirement(amount))
            {
                UserCurrencyModel currency = this.GetCurrency();
                if (currency == null)
                {
                    return false;
                }

                if (!userData.HasCurrencyAmount(currency, amount))
                {
                    return false;
                }
                userData.SubtractCurrencyAmount(currency, amount);
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

            UserCurrencyModel currency = this.GetCurrency();
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

            UserCurrencyModel currency = this.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            UserRankViewModel rank = this.RequiredRank;
            if (rank == null)
            {
                return false;
            }

            if (!userData.HasCurrencyAmount(currency, rank.MinimumPoints))
            {
                return false;
            }

            UserCurrencyDataViewModel userCurrencyData = userData.GetCurrency(currency);
            if (this.MustEqual && userCurrencyData.GetRank() != rank && !userData.IsCurrencyRankExempt)
            {
                return false;
            }

            return true;
        }

        public async Task SendCurrencyNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0}-{1} {2} to do this",
                        this.RequiredAmount, this.MaximumAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to do this",
                        this.RequiredAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
            }
        }

        public async Task SendCurrencyNotMetWhisper(UserViewModel user, int amount)
        {
            if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0}-{1} {2} to do this",
                        this.RequiredAmount, this.MaximumAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to do this",
                        this.RequiredAmount, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                }
            }
        }

        public async Task SendRankNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
            {
                await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required rank of {0} ({1} {2}) to do this",
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
