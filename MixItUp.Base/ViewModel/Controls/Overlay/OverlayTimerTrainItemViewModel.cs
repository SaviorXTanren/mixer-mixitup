using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayTimerTrainItemViewModel : OverlayHTMLTemplateItemViewModelBase
    {
        public string MinimumSecondsToShowString
        {
            get { return this.minimumSecondsToShow.ToString(); }
            set
            {
                this.minimumSecondsToShow = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int minimumSecondsToShow;

        public string SizeString
        {
            get { return this.size.ToString(); }
            set
            {
                this.size = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int size;

        public string Font
        {
            get { return this.font; }
            set
            {
                this.font = value;
                this.NotifyPropertyChanged();
            }
        }
        private string font;

        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.NotifyPropertyChanged();
            }
        }
        private string color;

        public string FollowBonusString
        {
            get { return this.followBonus.ToString(); }
            set
            {
                this.followBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double followBonus;

        public string HostBonusString
        {
            get { return this.hostBonus.ToString(); }
            set
            {
                this.hostBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double hostBonus;

        public string SubBonusString
        {
            get { return this.subBonus.ToString(); }
            set
            {
                this.subBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double subBonus;

        public string DonationBonusString
        {
            get { return this.donationBonus.ToString(); }
            set
            {
                this.donationBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double donationBonus;

        public OverlayTimerTrainItemViewModel()
        {
            this.Font = "Arial";
            this.size = 24;
            this.HTML = OverlayTimerTrainItemModel.HTMLTemplate;

            this.minimumSecondsToShow = 1;

            this.followBonus = 1.0;
            this.hostBonus = 1.0;
            this.subBonus = 10.0;
            this.donationBonus = 1.0;
        }

        public OverlayTimerTrainItemViewModel(OverlayTimerTrainItemModel item)
            : this()
        {
            this.minimumSecondsToShow = item.MinimumSecondsToShow;
            this.size = item.TextSize;
            this.Font = item.TextFont;
            this.Color = ColorSchemes.GetColorName(item.TextColor);

            this.followBonus = item.FollowBonus;
            this.hostBonus = item.HostBonus;
            this.subBonus = item.SubscriberBonus;
            this.donationBonus = item.DonationBonus;

            this.HTML = item.HTML;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.Color) && !string.IsNullOrEmpty(this.HTML) && this.size > 0 && this.minimumSecondsToShow > 0)
            {
                this.Color = ColorSchemes.GetColorCode(this.Color);

                return new OverlayTimerTrainItemModel(this.HTML, this.minimumSecondsToShow, this.Color, this.Font, this.size, this.followBonus, this.hostBonus,
                    this.subBonus, this.donationBonus);
            }
            return null;
        }
    }
}
