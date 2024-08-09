using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayVideoV3ViewModel : OverlayItemV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayVideoV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayVideoV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayVideoV3Model.DefaultJavascript; } }

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

        public string StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private string startTime = "0";

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

        public OverlayVideoV3ViewModel()
            : base(OverlayItemV3Type.Video)
        {
            this.SetCommands();
        }

        public OverlayVideoV3ViewModel(OverlayVideoV3Model item)
            : base(item)
        {
            this.FilePath = item.FilePath;
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = (int)(item.Volume * 100);
            this.StartTime = item.StartTime;
            this.Loop = item.Loop;

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
            OverlayVideoV3Model result = new OverlayVideoV3Model()
            {
                FilePath = this.FilePath,
                Width = this.width,
                Height = this.height,
                Volume = ((double)this.Volume) / 100.0,
                StartTime = this.StartTime,
                Loop = this.Loop
            };

            return result;
        }

        private void SetCommands()
        {
            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                IEnumerable<string> filepaths = ServiceManager.Get<IFileService>().ShowMultiselectOpenFileDialog(ServiceManager.Get<IFileService>().VideoFileFilter());
                if (filepaths != null)
                {
                    this.FilePath = string.Join("|", filepaths);
                }
            });
        }
    }
}