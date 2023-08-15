using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTwitchClipV3ViewModel : OverlayItemV3ViewModelBase
    {
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

        public OverlayTwitchClipV3ViewModel() : base(OverlayItemV3Type.TwitchClip) { }

        public OverlayTwitchClipV3ViewModel(OverlayTwitchClipV3Model item)
            : base(item)
        {
            this.width = item.Width;
            this.height = item.Height;
        }

        public override Result Validate()
        {
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
            OverlayTwitchClipV3Model result = new OverlayTwitchClipV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                Width = this.width,
                Height = this.height,
            };

            return result;
        }
    }
}