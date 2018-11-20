using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEventListControl.xaml
    /// </summary>
    public partial class OverlayEventListControl : OverlayItemControl
    {
        private OverlayEventList item;

        public OverlayEventListControl()
        {
            InitializeComponent();
        }

        public OverlayEventListControl(OverlayEventList item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayEventList)item;

            this.ShowFollowsCheckBox.IsChecked = this.item.ItemTypes.Contains(EventListItemTypeEnum.Followers);
            this.ShowHostsCheckBox.IsChecked = this.item.ItemTypes.Contains(EventListItemTypeEnum.Hosts);
            this.ShowSubsResubsCheckBox.IsChecked = this.item.ItemTypes.Contains(EventListItemTypeEnum.Subscribers);
            this.ShowDonationsCheckBox.IsChecked = this.item.ItemTypes.Contains(EventListItemTypeEnum.Donations);
            this.ShowMilestonesCheckBox.IsChecked = this.item.ItemTypes.Contains(EventListItemTypeEnum.Milestones);

            this.TotalToShowTextBox.Text = this.item.TotalToShow.ToString();
            this.ResetOnLoadCheckBox.IsChecked = this.item.ResetOnLoad;

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.TextFontComboBox.Text = this.item.TextFont;

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

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.AddEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.AddEventAnimation);
            this.RemoveEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.RemoveEventAnimation);

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            List<EventListItemTypeEnum> eventTypes = new List<EventListItemTypeEnum>();
            if (this.ShowFollowsCheckBox.IsChecked.GetValueOrDefault()) { eventTypes.Add(EventListItemTypeEnum.Followers); }
            if (this.ShowHostsCheckBox.IsChecked.GetValueOrDefault()) { eventTypes.Add(EventListItemTypeEnum.Hosts); }
            if (this.ShowSubsResubsCheckBox.IsChecked.GetValueOrDefault()) { eventTypes.Add(EventListItemTypeEnum.Subscribers); }
            if (this.ShowDonationsCheckBox.IsChecked.GetValueOrDefault()) { eventTypes.Add(EventListItemTypeEnum.Donations); }
            if (this.ShowMilestonesCheckBox.IsChecked.GetValueOrDefault()) { eventTypes.Add(EventListItemTypeEnum.Milestones); }

            if (eventTypes.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.TotalToShowTextBox.Text) || !int.TryParse(this.TotalToShowTextBox.Text, out int totalToShow) || totalToShow <= 0)
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

            string textColor = this.TextColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(textColor))
            {
                textColor = ColorSchemes.ColorSchemeDictionary[textColor];
            }

            string textFont = this.TextFontComboBox.Text;

            if (string.IsNullOrEmpty(this.WidthTextBox.Text) || !int.TryParse(this.WidthTextBox.Text, out int width) ||
                string.IsNullOrEmpty(this.HeightTextBox.Text) || !int.TryParse(this.HeightTextBox.Text, out int height))
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            OverlayEffectEntranceAnimationTypeEnum addEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.AddEventAnimationComboBox.SelectedItem);
            OverlayEffectExitAnimationTypeEnum removeEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.RemoveEventAnimationComboBox.SelectedItem);

            return new OverlayEventList(this.HTMLText.Text, eventTypes, totalToShow, this.ResetOnLoadCheckBox.IsChecked.GetValueOrDefault(), textFont, width, height,
                borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation);
        }

        protected override Task OnLoaded()
        {
            this.TotalToShowTextBox.Text = "5";

            this.WidthTextBox.Text = "0";
            this.HeightTextBox.Text = "0";

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.BorderColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.AddEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.AddEventAnimationComboBox.SelectedIndex = 0;
            this.RemoveEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.RemoveEventAnimationComboBox.SelectedIndex = 0;

            this.HTMLText.Text = OverlayEventList.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
