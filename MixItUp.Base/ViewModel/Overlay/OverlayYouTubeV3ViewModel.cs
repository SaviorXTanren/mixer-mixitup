using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayYouTubeV3ViewModel : OverlayItemV3ViewModelBase
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

        public OverlayYouTubeV3ViewModel(bool addDefaultAnimation = false) : base(OverlayItemV3Type.YouTube, addDefaultAnimation) { }

        public OverlayYouTubeV3ViewModel(OverlayYouTubeV3Model item)
            : base(item)
        {
            this.VideoID = item.VideoID;
            this.StartTime = item.StartTime;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
        }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.VideoID))
            {
                return new Result(Resources.OverlayYouTubeMissingVideo);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayYouTubeV3Model result = new OverlayYouTubeV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                VideoID = videoID,
                StartTime = this.StartTime,
                Width = this.width,
                Height = this.height,
                Volume = this.Volume,
            };

            return result;
        }
    }
}