using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayTimerTrainItemViewModel : OverlayCustomHTMLItemViewModelBase
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

        public string SparkBonusString
        {
            get { return this.sparkBonus.ToString(); }
            set
            {
                this.sparkBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double sparkBonus;

        public string EmberBonusString
        {
            get { return this.emberBonus.ToString(); }
            set
            {
                this.emberBonus = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private double emberBonus;

        public OverlayTimerTrainItemViewModel()
        {
            this.Font = "Arial";
            this.HTML = OverlayTimerTrain.HTMLTemplate;

            this.followBonus = 1.0;
            this.hostBonus = 1.0;
            this.subBonus = 10.0;
            this.donationBonus = 1.0;
            this.sparkBonus = 0.01;
            this.emberBonus = 0.1;
        }

        public OverlayTimerTrainItemViewModel(OverlayTimerTrain item)
            : this()
        {
            this.minimumSecondsToShow = item.MinimumSecondsToShow;
            this.size = item.TextSize;
            this.Font = item.TextFont;

            this.Color = item.TextColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.Color))
            {
                this.Color = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.Color)).Key;
            }

            this.followBonus = item.FollowBonus;
            this.hostBonus = item.HostBonus;
            this.subBonus = item.SubscriberBonus;
            this.donationBonus = item.DonationBonus;
            this.sparkBonus = item.SparkBonus;
            this.emberBonus = item.EmberBonus;

            this.HTML = item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.Color) && !string.IsNullOrEmpty(this.HTML) && this.size > 0 && this.minimumSecondsToShow > 0)
            {
                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.Color))
                {
                    this.Color = ColorSchemes.HTMLColorSchemeDictionary[this.Color];
                }
                return new OverlayTimerTrain(this.HTML, this.minimumSecondsToShow, this.Color, this.Font, this.size, this.followBonus, this.hostBonus,
                    this.subBonus, this.donationBonus, this.sparkBonus, this.emberBonus);
            }
            return null;
        }
    }
}
