using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayYouTubeItemViewModel : OverlayItemViewModelBase
    {
        public string VideoID
        {
            get { return this.videoID; }
            set
            {
                this.videoID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string videoID;

        public string StartTimeString
        {
            get { return this.startTime.ToString(); }
            set
            {
                this.startTime = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int startTime;

        public string WidthString
        {
            get { return this.width.ToString(); }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public string HeightString
        {
            get { return this.height.ToString(); }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int height;

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int volume;

        public OverlayYouTubeItemViewModel()
        {
            this.width = OverlayVideoItem.DefaultWidth;
            this.height = OverlayVideoItem.DefaultHeight;
            this.Volume = 100;
        }

        public OverlayYouTubeItemViewModel(OverlayYouTubeItem item)
            : this()
        {
            this.VideoID = item.VideoID;
            this.startTime = item.StartTime;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
        }

        public OverlayYouTubeItemViewModel(OverlayYouTubeItemModel item)
            : this()
        {
            this.VideoID = item.FilePath;
            this.startTime = item.StartTime;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.VideoID) && this.width > 0 && this.height > 0 && this.startTime >= 0)
            {
                string videoID = this.VideoID;
                videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                videoID = videoID.Replace("https://youtu.be/", "");
                if (videoID.Contains("&"))
                {
                    videoID = videoID.Substring(0, videoID.IndexOf("&"));
                }
                return new OverlayYouTubeItem(videoID, this.startTime, this.width, this.height, this.volume);
            }
            return null;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.VideoID) && this.width > 0 && this.height > 0 && this.startTime >= 0)
            {
                string videoID = this.VideoID;
                videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                videoID = videoID.Replace("https://youtu.be/", "");
                if (videoID.Contains("&"))
                {
                    videoID = videoID.Substring(0, videoID.IndexOf("&"));
                }
                return new OverlayYouTubeItemModel(videoID, this.width, this.height, this.startTime, this.volume);
            }
            return null;
        }
    }
}
