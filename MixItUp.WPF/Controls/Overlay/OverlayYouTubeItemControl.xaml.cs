using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayYouTubeItemControl.xaml
    /// </summary>
    public partial class OverlayYouTubeItemControl : OverlayItemControl
    {
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

        private OverlayYouTubeItem item;

        public OverlayYouTubeItemControl()
        {
            InitializeComponent();
        }

        public OverlayYouTubeItemControl(OverlayYouTubeItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayYouTubeItem)item;
            this.YoutubeVideoIDTextBox.Text = this.item.ID;
            this.YoutubeStartTimeTextBox.Text = this.item.StartTime.ToString();
            this.YouTubeWidthTextBox.Text = this.item.Width.ToString();
            this.YouTubeHeightTextBox.Text = this.item.Height.ToString();
            this.YouTubeVolumeSlider.Value = this.item.Volume;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.YoutubeVideoIDTextBox.Text))
            {
                string videoID = this.YoutubeVideoIDTextBox.Text;
                videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                videoID = videoID.Replace("https://youtu.be/", "");
                if (videoID.Contains("&"))
                {
                    videoID = videoID.Substring(0, videoID.IndexOf("&"));
                }

                if (int.TryParse(this.YoutubeStartTimeTextBox.Text, out int startTime))
                {
                    int width;
                    int height;
                    if (int.TryParse(this.YouTubeWidthTextBox.Text, out width) && width > 0 &&
                        int.TryParse(this.YouTubeHeightTextBox.Text, out height) && height > 0)
                    {
                        return new OverlayYouTubeItem(videoID, startTime, width, height, (int)this.YouTubeVolumeSlider.Value);
                    }
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.YoutubeStartTimeTextBox.Text = "0";
            this.YouTubeWidthTextBox.Text = OverlayVideoItem.DefaultWidth.ToString();
            this.YouTubeHeightTextBox.Text = OverlayVideoItem.DefaultHeight.ToString();
            this.YouTubeVolumeSlider.Value = 100;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
