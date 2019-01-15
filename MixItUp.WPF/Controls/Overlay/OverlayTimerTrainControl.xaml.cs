using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTimerTrainControl.xaml
    /// </summary>
    public partial class OverlayTimerTrainControl : OverlayItemControl
    {
        private OverlayTimerTrain item;

        public OverlayTimerTrainControl()
        {
            InitializeComponent();
        }

        public OverlayTimerTrainControl(OverlayTimerTrain item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayTimerTrain)item;

            this.MinimumSecondsTextBox.Text = this.item.MinimumSecondsToShow.ToString();
            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }
            this.TextFontComboBox.Text = this.item.TextFont;
            this.TextSizeComboBox.Text = this.item.TextSize.ToString();

            this.FollowBonusTextBox.Text = this.item.FollowBonus.ToString();
            this.HostBonusTextBox.Text = this.item.HostBonus.ToString();
            this.SubBonusTextBox.Text = this.item.SubscriberBonus.ToString();
            this.DonationBonusTextBox.Text = this.item.DonationBonus.ToString();
            this.SparkBonusTextBox.Text = this.item.SparkBonus.ToString();

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!int.TryParse(this.MinimumSecondsTextBox.Text, out int minSecondsToShow) || minSecondsToShow <= 0)
            {
                return null;
            }

            string textColor = this.TextColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(textColor))
            {
                textColor = ColorSchemes.ColorSchemeDictionary[textColor];
            }

            if (string.IsNullOrEmpty(this.TextFontComboBox.Text))
            {
                return null;
            }

            if (!int.TryParse(this.TextSizeComboBox.Text, out int size) || size <= 0)
            {
                return null;
            }

            if (!double.TryParse(this.FollowBonusTextBox.Text, out double followBonus) || followBonus < 0.0)
            {
                return null;
            }

            if (!double.TryParse(this.HostBonusTextBox.Text, out double hostBonus) || hostBonus < 0.0)
            {
                return null;
            }

            if (!double.TryParse(this.SubBonusTextBox.Text, out double subBonus) || subBonus < 0.0)
            {
                return null;
            }

            if (!double.TryParse(this.DonationBonusTextBox.Text, out double donationBonus) || donationBonus < 0.0)
            {
                return null;
            }

            if (!double.TryParse(this.SparkBonusTextBox.Text, out double sparkBonus) || sparkBonus < 0.0)
            {
                return null;
            }

            if (!double.TryParse(this.EmberBonusTextBox.Text, out double emberBonus) || emberBonus < 0.0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            return new OverlayTimerTrain(this.HTMLText.Text, minSecondsToShow, textColor, this.TextFontComboBox.Text, size, followBonus, hostBonus,
                subBonus, donationBonus, sparkBonus, emberBonus);
        }

        protected override Task OnLoaded()
        {
            this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.TextSizeComboBox.ItemsSource = OverlayTextItemControl.sampleFontSize.Select(f => f.ToString());

            this.MinimumSecondsTextBox.Text = "5";
            this.TextFontComboBox.Text = "Arial";

            this.FollowBonusTextBox.Text = "1.0";
            this.HostBonusTextBox.Text = "1.0";
            this.SubBonusTextBox.Text = "10.0";
            this.DonationBonusTextBox.Text = "1.0";
            this.SparkBonusTextBox.Text = "0.01";
            this.EmberBonusTextBox.Text = "0.1";

            this.HTMLText.Text = OverlayTimer.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
