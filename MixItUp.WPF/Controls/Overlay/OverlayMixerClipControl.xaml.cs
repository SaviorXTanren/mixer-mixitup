using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayMixerClipControl.xaml
    /// </summary>
    public partial class OverlayMixerClipControl : OverlayItemControl
    {
        private OverlayMixerClip item;

        public OverlayMixerClipControl()
        {
            InitializeComponent();
        }

        public OverlayMixerClipControl(OverlayMixerClip item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayMixerClip)item;

            this.VideoWidthTextBox.Text = this.item.Width.ToString();
            this.VideoHeightTextBox.Text = this.item.Height.ToString();
            this.VideoVolumeSlider.Value = this.item.Volume;

            this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.EntranceAnimation);
            this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.ExitAnimation);
        }

        public override OverlayItemBase GetItem()
        {
            int width;
            int height;
            if (int.TryParse(this.VideoWidthTextBox.Text, out width) && width > 0 &&
                int.TryParse(this.VideoHeightTextBox.Text, out height) && height > 0)
            {
                OverlayEffectEntranceAnimationTypeEnum entranceEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.EntranceAnimationComboBox.SelectedItem);
                OverlayEffectExitAnimationTypeEnum exitEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.ExitAnimationComboBox.SelectedItem);

                return new OverlayMixerClip(width, height, (int)this.VideoVolumeSlider.Value, entranceEventAnimation, exitEventAnimation);
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.VideoVolumeSlider.Value = 100;
            this.VideoWidthTextBox.Text = OverlayVideoItem.DefaultWidth.ToString();
            this.VideoHeightTextBox.Text = OverlayVideoItem.DefaultHeight.ToString();

            this.EntranceAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.EntranceAnimationComboBox.SelectedIndex = 0;
            this.ExitAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.ExitAnimationComboBox.SelectedIndex = 0;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
