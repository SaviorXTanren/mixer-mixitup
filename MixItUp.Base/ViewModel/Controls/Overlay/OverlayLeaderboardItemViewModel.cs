using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayLeaderboardItemViewModel : OverlayListItemViewModelBase
    {
        public IEnumerable<string> LeaderboardTypeStrings { get; set; } = EnumHelper.GetEnumNames<OverlayLeaderboardListItemTypeEnum>();
        public string LeaderboardTypeString
        {
            get { return EnumHelper.GetEnumName(this.leaderboardType); }
            set
            {
                this.leaderboardType = EnumHelper.GetEnumValueFromString<OverlayLeaderboardListItemTypeEnum>(value);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsCurrencyRankType");
                this.NotifyPropertyChanged("IsSparksEmbersType");
                this.NotifyPropertyChanged("IsDonationsType");
            }
        }
        private OverlayLeaderboardListItemTypeEnum leaderboardType;

        public bool IsCurrencyRankType { get { return this.leaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank; } }
        public bool IsSparksEmbersType { get { return this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks || this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Embers; } }

        public IEnumerable<string> SparksEmbersDateStrings { get; set; } = EnumHelper.GetEnumNames<OverlayLeaderboardListItemDateRangeEnum>();
        public string SparksEmbersDateString
        {
            get { return EnumHelper.GetEnumName(this.sparksEmbersDate); }
            set
            {
                this.sparksEmbersDate = EnumHelper.GetEnumValueFromString<OverlayLeaderboardListItemDateRangeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayLeaderboardListItemDateRangeEnum sparksEmbersDate;

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
            this.HTML = OverlayLeaderboardListItemModel.HTMLTemplate;
        }

        public OverlayLeaderboardItemViewModel(OverlayLeaderboardListItemModel item)
            : base(item.TotalToShow, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.Effects.EntranceAnimation, item.Effects.ExitAnimation, item.HTML)
        {
            this.leaderboardType = item.LeaderboardType;
            if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(item.CurrencyID))
                {
                    this.CurrencyRank = ChannelSession.Settings.Currencies[item.CurrencyID];
                }
            }
            else if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks || this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Embers)
            {
                this.sparksEmbersDate = item.LeaderboardDateRange;
            }
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.Validate() && !string.IsNullOrEmpty(this.LeaderboardTypeString))
            {
                this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                this.BorderColor = ColorSchemes.GetColorCode(this.BorderColor);
                this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
                {
                    if (this.CurrencyRank != null)
                    {
                        return new OverlayLeaderboardListItemModel(this.HTML, this.leaderboardType, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor,
                            this.TextColor, this.entranceAnimation, this.exitAnimation, this.CurrencyRank);
                    }
                }
                else if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks || this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Embers)
                {
                    if (!string.IsNullOrEmpty(this.SparksEmbersDateString))
                    {
                        return new OverlayLeaderboardListItemModel(this.HTML, this.leaderboardType, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor,
                            this.TextColor, this.entranceAnimation, this.exitAnimation, this.sparksEmbersDate);
                    }
                }
                else
                {
                    return new OverlayLeaderboardListItemModel(this.HTML, this.leaderboardType, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor,
                            this.TextColor, this.entranceAnimation, this.exitAnimation);
                }
            }
            return null;
        }
    }
}
