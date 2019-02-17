using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public abstract class RemoteButtonItemViewModelBase : RemoteItemViewModelBase
    {
        private new RemoteButtonItemModelBase model;

        public RemoteButtonItemViewModelBase(RemoteButtonItemModelBase model)
            : base(model)
        {
            this.model = model;

            this.BackgroundImageBrowseCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.BackgroundImage = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public IEnumerable<string> PreDefinedColors { get { return ColorSchemes.WPFColorSchemeDictionary; } }

        public string BackgroundColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.BackgroundColor))
                {
                    return this.model.BackgroundColor;
                }
                return "Transparent";
            }
            set
            {
                this.model.BackgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }

        public string TextColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.TextColor))
                {
                    return this.model.TextColor;
                }
                return "Black";
            }
            set
            {
                this.model.TextColor = value;
                this.NotifyPropertyChanged();
            }
        }

        public string BackgroundImage
        {
            get { return this.model.ImagePath; }
            set
            {
                this.model.ImagePath = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasBackgroundImage");
                this.NotifyPropertyChanged("DoesNotHaveBackgroundImage");
            }
        }

        public ICommand BackgroundImageBrowseCommand { get; private set; }

        public bool HasBackgroundImage { get { return !string.IsNullOrEmpty(this.BackgroundImage) && ChannelSession.Services.FileService.FileExists(this.BackgroundImage); } }
        public bool DoesNotHaveBackgroundImage { get { return !this.HasBackgroundImage; } }
    }
}
