using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayImageItemViewModel : OverlayItemViewModelBase
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

        public override bool SupportsRefreshUpdating { get { return true; } }

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayImageItemViewModel()
        {
            this.BrowseFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.FilePath = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public OverlayImageItemViewModel(OverlayImageItemModel item)
            : this()
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.FilePath) && this.width >= 0 && this.height >= 0)
            {
                return new OverlayImageItemModel(this.FilePath, width, height);
            }
            return null;
        }
    }
}
