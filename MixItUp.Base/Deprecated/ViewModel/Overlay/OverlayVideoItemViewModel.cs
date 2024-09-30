using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
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

        public bool Loop
        {
            get { return this.loop; }
            set
            {
                this.loop = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool loop;

        public override bool SupportsRefreshUpdating { get { return true; } }

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayVideoItemViewModel()
        {
            this.width = OverlayVideoItemModel.DefaultWidth;
            this.height = OverlayVideoItemModel.DefaultHeight;
            this.Volume = 100;

            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().VideoFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.FilePath = filePath;
                }
                return Task.CompletedTask;
            });
        }

        public OverlayVideoItemViewModel(OverlayVideoItemModel item)
            : this()
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
            this.Loop = item.Loop;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.FilePath) && this.width > 0 && this.height > 0)
            {
                return new OverlayVideoItemModel(this.FilePath, this.width, this.height, this.Volume, this.Loop);
            }
            return null;
        }
    }
}
