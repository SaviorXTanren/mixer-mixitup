using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayLeaderboardItemViewModel : OverlayListItemViewModelBase
    {
        public IEnumerable<string> LeaderboardTypeStrings { get; set; } = EnumHelper.GetEnumNames<LeaderboardTypeEnum>();
        public string LeaderboardTypeString
        {
            get { return EnumHelper.GetEnumName(this.leaderboardType); }
            set
            {
                this.leaderboardType = EnumHelper.GetEnumValueFromString<LeaderboardTypeEnum>(value);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsCurrencyRankType");
                this.NotifyPropertyChanged("IsSparksEmbersType");
                this.NotifyPropertyChanged("IsDonationsType");
            }
        }
        private LeaderboardTypeEnum leaderboardType;

        public bool IsCurrencyRankType { get { return this.leaderboardType == LeaderboardTypeEnum.CurrencyRank; } }
        public bool IsSparksEmbersType { get { return this.leaderboardType == LeaderboardTypeEnum.Sparks || this.leaderboardType == LeaderboardTypeEnum.Embers; } }
        public bool IsDonationsType { get { return this.leaderboardType == LeaderboardTypeEnum.Donations; } }

        public IEnumerable<string> SparksEmbersDateStrings { get; set; } = EnumHelper.GetEnumNames<LeaderboardSparksEmbersDateEnum>();
        public string SparksEmbersDateString
        {
            get { return EnumHelper.GetEnumName(this.sparksEmbersDate); }
            set
            {
                this.sparksEmbersDate = EnumHelper.GetEnumValueFromString<LeaderboardSparksEmbersDateEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private LeaderboardSparksEmbersDateEnum sparksEmbersDate;

        public IEnumerable<UserCurrencyViewModel> CurrencyRanks { get; set; } = ChannelSession.Settings.Currencies.Values.ToList();
        public UserCurrencyViewModel CurrencyRank
        {
            get { return this.currencyRank; }
            set
            {
                this.currencyRank = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserCurrencyViewModel currencyRank;

        public OverlayLeaderboardItemViewModel()
            : base()
        {
            this.HTML = OverlayLeaderboard.HTMLTemplate;
        }

        public OverlayLeaderboardItemViewModel(OverlayLeaderboard item)
            : base(item.TotalToShow, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.AddEventAnimation, item.RemoveEventAnimation, item.HTMLText)
        {
            this.leaderboardType = item.LeaderboardType;
            if (this.leaderboardType == LeaderboardTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(item.CurrencyID))
                {
                    this.CurrencyRank = ChannelSession.Settings.Currencies[item.CurrencyID];
                }
            }
            else if (this.leaderboardType == LeaderboardTypeEnum.Sparks || this.leaderboardType == LeaderboardTypeEnum.Embers)
            {
                this.sparksEmbersDate = item.DateRange;
            }
        }

        public override OverlayItemBase GetItem()
        {
            if (this.Validate() && !string.IsNullOrEmpty(this.LeaderboardTypeString))
            {
                if (this.leaderboardType == LeaderboardTypeEnum.CurrencyRank)
                {
                    if (this.CurrencyRank != null)
                    {
                        return new OverlayLeaderboard(this.HTML, this.leaderboardType, totalToShow, this.BorderColor, this.BackgroundColor, this.TextColor, this.Font, this.width,
                            this.height, this.entranceAnimation, this.exitAnimation, this.CurrencyRank);
                    }
                }
                else if (this.leaderboardType == LeaderboardTypeEnum.Sparks || this.leaderboardType == LeaderboardTypeEnum.Embers)
                {
                    if (!string.IsNullOrEmpty(this.SparksEmbersDateString))
                    {
                        return new OverlayLeaderboard(this.HTML, this.leaderboardType, totalToShow, this.BorderColor, this.BackgroundColor, this.TextColor, this.Font, this.width,
                            this.height, this.entranceAnimation, this.exitAnimation, this.sparksEmbersDate);
                    }
                }
                else
                {
                    return new OverlayLeaderboard(this.HTML, this.leaderboardType, totalToShow, this.BorderColor, this.BackgroundColor, this.TextColor, this.Font, this.width,
                        this.height, this.entranceAnimation, this.exitAnimation);
                }
            }
            return null;
        }
    }
}
