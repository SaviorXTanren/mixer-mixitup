using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWebPageItemV3ViewModel : OverlayItemV3ViewModelBase
    {
        public string URL
        {
            get { return this.url; }
            set
            {
                this.url = value;
                this.NotifyPropertyChanged();
            }
        }
        private string url;

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

        public OverlayWebPageItemV3ViewModel() : base(OverlayItemV3Type.WebPage) { }

        public OverlayWebPageItemV3ViewModel(OverlayWebPageItemV3Model item)
            : base(item)
        {
            this.URL = item.URL;
            this.width = item.Width;
            this.height = item.Height;
        }

        public OverlayWebPageItemV3Model GetItem()
        {
            OverlayWebPageItemV3Model result = new OverlayWebPageItemV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                URL = this.URL,
                Width = this.width,
                Height = this.height,
            };

            return result;
        }
    }
}