using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
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

        public override bool SupportsRefreshUpdating { get { return true; } }

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayWebPageItemViewModel()
        {
            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().HTMLFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.URL = filePath;
                }
            });
        }

        public OverlayWebPageItemViewModel(OverlayWebPageItemModel item)
            : this()
        {
            this.URL = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.URL) && this.width > 0 && this.height > 0)
            {
                return new OverlayWebPageItemModel(this.URL, this.width, this.height);
            }
            return null;
        }
    }
}
