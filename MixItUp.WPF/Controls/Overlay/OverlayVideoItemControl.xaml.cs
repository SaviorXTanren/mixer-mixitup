using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayVideoItemControl.xaml
    /// </summary>
    public partial class OverlayVideoItemControl : OverlayItemControl
    {
        private OverlayVideoItem item;

        public OverlayVideoItemControl()
        {
            InitializeComponent();
        }

        public OverlayVideoItemControl(OverlayVideoItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayVideoItem)item;
            this.VideoFilePathTextBox.Text = this.item.FilePath;
            this.VideoWidthTextBox.Text = this.item.Width.ToString();
            this.VideoHeightTextBox.Text = this.item.Height.ToString();
            this.VideoVolumeSlider.Value = this.item.Volume;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.VideoFilePathTextBox.Text))
            {
                int width;
                int height;
                if (int.TryParse(this.VideoWidthTextBox.Text, out width) && width > 0 &&
                    int.TryParse(this.VideoHeightTextBox.Text, out height) && height > 0)
                {
                    return new OverlayVideoItem(this.VideoFilePathTextBox.Text, width, height, (int)this.VideoVolumeSlider.Value);
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.VideoVolumeSlider.Value = 100;
            this.VideoWidthTextBox.Text = OverlayVideoItem.DefaultWidth.ToString();
            this.VideoHeightTextBox.Text = OverlayVideoItem.DefaultHeight.ToString();

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void VideoFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.VideoFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.VideoFilePathTextBox.Text = filePath;
            }
        }
    }
}
