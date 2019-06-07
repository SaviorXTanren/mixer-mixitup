using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayWebPageItemViewModel : OverlayItemViewModelBase
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

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayWebPageItemViewModel()
        {
            this.BrowseFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.HTMLFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.URL = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public OverlayWebPageItemViewModel(OverlayWebPageItem item)
            : this()
        {
            this.URL = item.URL;
            this.width = item.Width;
            this.height = item.Height;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.URL) && this.width > 0 && this.height > 0)
            {
                return new OverlayWebPageItem(this.URL, this.width, this.height);
            }
            return null;
        }
    }
}
