using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayImageV3ViewModel : OverlayItemV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayImageV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayImageV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayImageV3Model.DefaultJavascript; } }

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

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlayImageV3ViewModel()
            : base(OverlayItemV3Type.Image)
        {
            this.SetCommands();
        }

        public OverlayImageV3ViewModel(OverlayImageV3Model item)
            : base(item)
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;

            this.SetCommands();
        }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                return new Result(Resources.OverlayMissingFilePath);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayImageV3Model result = new OverlayImageV3Model()
            {
                FilePath = this.FilePath,
                Width = this.width,
                Height = this.height,
            };

            return result;
        }

        private void SetCommands()
        {
            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                IEnumerable<string> filepaths = ServiceManager.Get<IFileService>().ShowMultiselectOpenFileDialog(ServiceManager.Get<IFileService>().ImageFileFilter());
                if (filepaths != null)
                {
                    this.FilePath = string.Join("|", filepaths);
                }
            });
        }
    }
}
