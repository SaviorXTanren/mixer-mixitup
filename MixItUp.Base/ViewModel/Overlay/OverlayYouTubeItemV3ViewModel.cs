using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayYouTubeItemV3ViewModel : OverlayItemV3ViewModelBase
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

        public int StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private int startTime;

        public string Width
        {
            get { return this.width > 0 ? this.width.ToString() : string.Empty; }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
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
        private int volume = 100;

        public OverlayYouTubeItemV3ViewModel() : base(OverlayItemV3Type.YouTube) { }

        public OverlayYouTubeItemV3ViewModel(OverlayYouTubeItemV3Model item)
            : base(item)
        {
            this.VideoID = item.VideoID;
            this.StartTime = item.StartTime;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = (int)(item.Volume * 100);
        }

        public OverlayYouTubeItemV3Model GetItem()
        {
            string videoID = this.VideoID;
            videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
            videoID = videoID.Replace("https://youtu.be/", "");
            if (videoID.Contains("&"))
            {
                videoID = videoID.Substring(0, videoID.IndexOf("&"));
            }

            OverlayYouTubeItemV3Model result = new OverlayYouTubeItemV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                VideoID = videoID,
                StartTime = this.StartTime,
                Width = this.width,
                Height = this.height,
                Volume = ((double)this.Volume) / 100.0,
            };

            return result;
        }
    }
}