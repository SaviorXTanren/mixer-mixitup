using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayStreamBossControl.xaml
    /// </summary>
    public partial class OverlayStreamBossControl : OverlayItemControl
    {
        private OverlayStreamBoss item;

        public OverlayStreamBossControl()
        {
            InitializeComponent();
        }

        public OverlayStreamBossControl(OverlayStreamBoss item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayStreamBoss)item;

            this.StartingHealthTextBox.Text = this.item.StartingHealth.ToString();

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.TextFontComboBox.Text = this.item.TextFont;
            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.BorderColorComboBox.Text = this.item.BorderColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.BorderColor))
            {
                this.BorderColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BorderColor)).Key;
            }

            this.BackgroundColorComboBox.Text = this.item.BackgroundColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.BackgroundColor))
            {
                this.BackgroundColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BackgroundColor)).Key;
            }

            this.ProgressColorComboBox.Text = this.item.ProgressColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.ProgressColor))
            {
                this.ProgressColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.ProgressColor)).Key;
            }

            this.DamageAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.DamageAnimation);
            this.NewBossAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.NewBossAnimation);

            this.FollowBonusTextBox.Text = this.item.FollowBonus.ToString();
            this.HostBonusTextBox.Text = this.item.HostBonus.ToString();
            this.SubBonusTextBox.Text = this.item.SubscriberBonus.ToString();
            this.DonationBonusTextBox.Text = this.item.DonationBonus.ToString();
            this.SparkBonusTextBox.Text = this.item.SparkBonus.ToString();

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!int.TryParse(this.StartingHealthTextBox.Text, out int startingHealth) || startingHealth <= 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.WidthTextBox.Text) || !int.TryParse(this.WidthTextBox.Text, out int width) ||
                string.IsNullOrEmpty(this.HeightTextBox.Text) || !int.TryParse(this.HeightTextBox.Text, out int height))
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

            string borderColor = this.BorderColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(borderColor))
            {
                borderColor = ColorSchemes.ColorSchemeDictionary[borderColor];
            }

            string backgroundColor = this.BackgroundColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(backgroundColor))
            {
                backgroundColor = ColorSchemes.ColorSchemeDictionary[backgroundColor];
            }

            string progressColor = this.ProgressColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(progressColor))
            {
                progressColor = ColorSchemes.ColorSchemeDictionary[progressColor];
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

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            OverlayEffectVisibleAnimationTypeEnum damageAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectVisibleAnimationTypeEnum>((string)this.DamageAnimationComboBox.SelectedItem);
            OverlayEffectVisibleAnimationTypeEnum newBossAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectVisibleAnimationTypeEnum>((string)this.NewBossAnimationComboBox.SelectedItem);

            return new OverlayStreamBoss(this.HTMLText.Text, startingHealth, width, height, textColor, this.TextFontComboBox.Text, borderColor, backgroundColor,
                progressColor, followBonus, hostBonus, subBonus, donationBonus, sparkBonus, damageAnimation, newBossAnimation);
        }

        protected override Task OnLoaded()
        {
            this.StartingHealthTextBox.Text = "5000";

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.TextColorComboBox.ItemsSource = this.BorderColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.ProgressColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.DamageAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectVisibleAnimationTypeEnum>();
            this.DamageAnimationComboBox.SelectedIndex = 0;
            this.NewBossAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectVisibleAnimationTypeEnum>();
            this.NewBossAnimationComboBox.SelectedIndex = 0;

            this.WidthTextBox.Text = "450";
            this.HeightTextBox.Text = "100";
            this.TextFontComboBox.Text = "Arial";

            this.FollowBonusTextBox.Text = "1.0";
            this.HostBonusTextBox.Text = "1.0";
            this.SubBonusTextBox.Text = "10.0";
            this.DonationBonusTextBox.Text = "1.0";
            this.SparkBonusTextBox.Text = "0.01";

            this.HTMLText.Text = OverlayStreamBoss.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
