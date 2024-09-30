using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayYouTubeV3ViewModel : OverlayItemV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayYouTubeV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayYouTubeV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayYouTubeV3Model.DefaultJavascript; } }

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

        public string StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private string startTime = "0";

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

        public OverlayYouTubeV3ViewModel() : base(OverlayItemV3Type.YouTube) { }

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

            if (string.IsNullOrEmpty(this.Width))
            {
                return new Result(Resources.OverlayWidthMustBeValidValue);
            }

            if (string.IsNullOrEmpty(this.Height))
            {
                return new Result(Resources.OverlayHeightMustBeValidValue);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayYouTubeV3Model result = new OverlayYouTubeV3Model()
            {
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