using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayLeaderboardListItemViewModel : OverlayListItemViewModelBase
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
                this.NotifyPropertyChanged("SupportsRefreshUpdating");
            }
        }
        private OverlayLeaderboardListItemTypeEnum leaderboardType;

        public bool IsCurrencyRankType { get { return this.leaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank; } }
        public bool IsSparksEmbersType { get { return this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks || this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Embers; } }
        public bool HasNewLeaderCommand { get { return this.NewLeaderCommand != null; } }
        public bool DoesNotHaveNewLeaderCommand { get { return !this.HasNewLeaderCommand; } }

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

        public IEnumerable<CurrencyModel> CurrencyRanks { get; set; } = ChannelSession.Settings.Currency.Values.ToList();
        public CurrencyModel CurrencyRank
        {
            get { return this.currencyRank; }
            set
            {
                this.currencyRank = value;
                this.NotifyPropertyChanged();
            }
        }
        private CurrencyModel currencyRank;

        public override bool SupportsRefreshUpdating { get { return true; } }

        private CustomCommand newLeaderCommand;
        public CustomCommand NewLeaderCommand
        {
            get { return this.newLeaderCommand; }
            set
            {
                this.newLeaderCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasNewLeaderCommand");
                this.NotifyPropertyChanged("DoesNotHaveNewLeaderCommand");
            }
        }

        public OverlayLeaderboardListItemViewModel()
            : base()
        {
            this.HTML = OverlayLeaderboardListItemModel.HTMLTemplate;
        }

        public OverlayLeaderboardListItemViewModel(OverlayLeaderboardListItemModel item)
            : base(item.TotalToShow, 0, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.Alignment, item.Effects.EntranceAnimation, item.Effects.ExitAnimation, item.HTML)
        {
            this.newLeaderCommand = item.NewLeaderCommand;
            this.leaderboardType = item.LeaderboardType;
            if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currency.ContainsKey(item.CurrencyID))
                {
                    this.CurrencyRank = ChannelSession.Settings.Currency[item.CurrencyID];
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
                            this.TextColor, this.alignment, this.entranceAnimation, this.exitAnimation, this.CurrencyRank, this.NewLeaderCommand);
                    }
                }
                else if (this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks || this.leaderboardType == OverlayLeaderboardListItemTypeEnum.Embers)
                {
                    if (!string.IsNullOrEmpty(this.SparksEmbersDateString))
                    {
                        return new OverlayLeaderboardListItemModel(this.HTML, this.leaderboardType, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor,
                            this.TextColor, this.alignment, this.entranceAnimation, this.exitAnimation, this.sparksEmbersDate, this.NewLeaderCommand);
                    }
                }
                else
                {
                    return new OverlayLeaderboardListItemModel(this.HTML, this.leaderboardType, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor,
                            this.TextColor, this.alignment, this.entranceAnimation, this.exitAnimation, this.NewLeaderCommand);
                }
            }
            return null;
        }
    }
}
