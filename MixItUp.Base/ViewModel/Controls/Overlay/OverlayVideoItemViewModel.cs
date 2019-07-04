using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayVideoItemViewModel : OverlayItemViewModelBase
    {
        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.filePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filePath;

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

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayVideoItemViewModel()
        {
            this.width = OverlayVideoItemModel.DefaultWidth;
            this.height = OverlayVideoItemModel.DefaultHeight;
            this.Volume = 100;

            this.BrowseFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.VideoFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.FilePath = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public OverlayVideoItemViewModel(OverlayVideoItemModel item)
            : this()
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.FilePath) && this.width > 0 && this.height > 0)
            {
                return new OverlayVideoItemModel(this.FilePath, this.width, this.height, this.volume);
            }
            return null;
        }
    }
}
