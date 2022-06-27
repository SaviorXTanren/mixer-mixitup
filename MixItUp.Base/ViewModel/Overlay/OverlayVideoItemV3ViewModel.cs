using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayVideoItemV3ViewModel : OverlayItemV3ViewModelBase
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

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayVideoItemV3ViewModel()
            : base(OverlayItemV3Type.Video)
        {
            this.SetCommands();
        }

        public OverlayVideoItemV3ViewModel(OverlayVideoItemV3Model item)
            : base(item)
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = (int)(item.Volume * 100);
            this.Loop = item.Loop;

            this.SetCommands();
        }

        public OverlayVideoItemV3Model GetItem()
        {
            OverlayVideoItemV3Model result = new OverlayVideoItemV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                FilePath = this.FilePath,
                Width = this.width,
                Height = this.height,
                Volume = ((double)this.Volume) / 100.0,
                Loop = this.Loop
            };

            return result;
        }

        private void SetCommands()
        {
            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                string filepath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().VideoFileFilter());
                if (!string.IsNullOrEmpty(filepath))
                {
                    this.FilePath = filepath;
                }
            });
        }
    }
}