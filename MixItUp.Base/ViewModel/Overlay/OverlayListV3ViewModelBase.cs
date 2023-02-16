using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayListV3ViewModelBase : OverlayVisualTextV3ViewModelBase
    {
        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public int ItemHeight
        {
            get { return this.itemHeight; }
            set
            {
                this.itemHeight = value;
                this.NotifyPropertyChanged();
            }
        }
        private int itemHeight;

        public int ItemWidth
        {
            get { return this.itemWidth; }
            set
            {
                this.itemWidth = value;
                this.NotifyPropertyChanged();
            }
        }
        private int itemWidth;

        public int MaxToShow
        {
            get { return this.maxToShow; }
            set
            {
                this.maxToShow = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxToShow;

        public OverlayListV3ViewModelBase(OverlayItemV3Type type)
            : base(type)
        {
            this.ItemHeight = 100;
            this.ItemWidth = 400;
            this.MaxToShow = 5;
        }

        public OverlayListV3ViewModelBase(OverlayListV3ModelBase item)
            : base(item)
        {
            this.ItemHeight = item.Height;
            this.MaxToShow = item.MaxToShow;
        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {
                if (this.ItemHeight <= 0)
                {
                    return new Result(Resources.OverlayHeightMustBeValidValue);
                }

                if (this.ItemWidth <= 0)
                {
                    return new Result(Resources.OverlayWidthMustBeValidValue);
                }

                if (this.MaxToShow <= 0)
                {
                    return new Result(Resources.OverlayMaxToShowMustBeValidValue);
                }
            }

            return result;
        }
    }
}
