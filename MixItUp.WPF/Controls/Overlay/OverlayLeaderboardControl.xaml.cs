using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayLeaderboardControl.xaml
    /// </summary>
    public partial class OverlayLeaderboardControl : OverlayItemControl
    {
        private OverlayLeaderboard item;

        public OverlayLeaderboardControl()
        {
            InitializeComponent();
        }

        public OverlayLeaderboardControl(OverlayLeaderboard item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayLeaderboard)item;

            this.LeaderboardTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.LeaderboardType);
            if (this.item.LeaderboardType == LeaderboardTypeEnum.CurrencyRank && ChannelSession.Settings.Currencies.ContainsKey(this.item.CurrencyID))
            {
                this.CurrencyRankComboBox.SelectedItem = ChannelSession.Settings.Currencies[this.item.CurrencyID];
            }

            if (this.item.LeaderboardType == LeaderboardTypeEnum.Sparks || this.item.LeaderboardType == LeaderboardTypeEnum.Embers)
            {
                this.SparkEmberDateComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.DateRange);
            }

            this.TotalToShowTextBox.Text = this.item.TotalToShow.ToString();

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.TextFontComboBox.Text = this.item.TextFont;

            this.BorderColorComboBox.Text = this.item.BorderColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.item.BorderColor))
            {
                this.BorderColorComboBox.Text = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BorderColor)).Key;
            }

            this.BackgroundColorComboBox.Text = this.item.BackgroundColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.item.BackgroundColor))
            {
                this.BackgroundColorComboBox.Text = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BackgroundColor)).Key;
            }

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.AddEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.AddEventAnimation);
            this.RemoveEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.RemoveEventAnimation);

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (this.LeaderboardTypeComboBox.SelectedIndex < 0)
            {
                return null;
            }
            LeaderboardTypeEnum leaderboardType = EnumHelper.GetEnumValueFromString<LeaderboardTypeEnum>((string)this.LeaderboardTypeComboBox.SelectedItem);

            if (string.IsNullOrEmpty(this.TotalToShowTextBox.Text) || !int.TryParse(this.TotalToShowTextBox.Text, out int totalToShow) || totalToShow <= 0)
            {
                return null;
            }

            string borderColor = this.BorderColorComboBox.Text;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(borderColor))
            {
                borderColor = ColorSchemes.HTMLColorSchemeDictionary[borderColor];
            }

            string backgroundColor = this.BackgroundColorComboBox.Text;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(backgroundColor))
            {
                backgroundColor = ColorSchemes.HTMLColorSchemeDictionary[backgroundColor];
            }

            string textColor = this.TextColorComboBox.Text;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(textColor))
            {
                textColor = ColorSchemes.HTMLColorSchemeDictionary[textColor];
            }

            if (string.IsNullOrEmpty(this.TextFontComboBox.Text))
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.WidthTextBox.Text) || !int.TryParse(this.WidthTextBox.Text, out int width) ||
                string.IsNullOrEmpty(this.HeightTextBox.Text) || !int.TryParse(this.HeightTextBox.Text, out int height))
            {
                return null;
            }

            OverlayEffectEntranceAnimationTypeEnum addEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.AddEventAnimationComboBox.SelectedItem);
            OverlayEffectExitAnimationTypeEnum removeEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.RemoveEventAnimationComboBox.SelectedItem);

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            if (leaderboardType == LeaderboardTypeEnum.CurrencyRank)
            {
                if (this.CurrencyRankComboBox.SelectedIndex < 0)
                {
                    return null;
                }
                return new OverlayLeaderboard(this.HTMLText.Text, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, this.TextFontComboBox.Text, width, height,
                    addEventAnimation, removeEventAnimation, (UserCurrencyViewModel)this.CurrencyRankComboBox.SelectedItem);
            }
            else if (leaderboardType == LeaderboardTypeEnum.Sparks || leaderboardType == LeaderboardTypeEnum.Embers)
            {
                if (this.SparkEmberDateComboBox.SelectedIndex < 0)
                {
                    return null;
                }
                return new OverlayLeaderboard(this.HTMLText.Text, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, this.TextFontComboBox.Text, width, height,
                    addEventAnimation, removeEventAnimation, EnumHelper.GetEnumValueFromString<LeaderboardSparksEmbersDateEnum>(this.SparkEmberDateComboBox.SelectedItem as string));
            }
            else
            {
                return new OverlayLeaderboard(this.HTMLText.Text, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, this.TextFontComboBox.Text, width, height,
                    addEventAnimation, removeEventAnimation);
            }
        }

        protected override Task OnLoaded()
        {
            this.LeaderboardTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<LeaderboardTypeEnum>();
            this.CurrencyRankComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
            this.SparkEmberDateComboBox.ItemsSource = EnumHelper.GetEnumNames<LeaderboardSparksEmbersDateEnum>();

            this.TotalToShowTextBox.Text = "5";

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.BorderColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.HTMLColorSchemeDictionary.Keys;

            this.AddEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.AddEventAnimationComboBox.SelectedIndex = 0;
            this.RemoveEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.RemoveEventAnimationComboBox.SelectedIndex = 0;

            this.WidthTextBox.Text = "400";
            this.HeightTextBox.Text = "100";
            this.TextFontComboBox.Text = "Arial";
            this.HTMLText.Text = OverlayLeaderboard.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void LeaderboardTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.DonationLeaderboardInfoTextBlock.Visibility = Visibility.Collapsed;
            this.CurrencyRankComboBox.Visibility = Visibility.Collapsed;
            this.SparkEmberDateComboBox.Visibility = Visibility.Collapsed;
            if (this.LeaderboardTypeComboBox.SelectedIndex >= 0)
            {
                LeaderboardTypeEnum leaderboardType = EnumHelper.GetEnumValueFromString<LeaderboardTypeEnum>((string)this.LeaderboardTypeComboBox.SelectedItem);
                if (leaderboardType == LeaderboardTypeEnum.Donations)
                {
                    this.DonationLeaderboardInfoTextBlock.Visibility = Visibility.Visible;
                }
                if (leaderboardType == LeaderboardTypeEnum.CurrencyRank)
                {
                    this.CurrencyRankComboBox.Visibility = Visibility.Visible;
                }
                if (leaderboardType == LeaderboardTypeEnum.Sparks || leaderboardType == LeaderboardTypeEnum.Embers)
                {
                    this.SparkEmberDateComboBox.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
